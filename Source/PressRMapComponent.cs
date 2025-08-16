using System.Collections.Generic;
using PressR.Features;
using PressR.Features.DirectHaul;
using PressR.Features.DirectHaulStorageMode;
using PressR.Features.StorageLens;
using PressR.Graphics;
using RimWorld.Planet;
using Verse;

namespace PressR
{
    public class PressRMapComponent : MapComponent
    {
        public IGraphicsManager GraphicsManager { get; }
        public DirectHaul DirectHaul { get; private set; }
        public DirectHaulStorageMode DirectHaulStorageMode { get; private set; }
        public StorageLens StorageLens { get; private set; }

        public bool IsActive { get; private set; }

        private readonly List<IPressRFeature> _features = [];
        private IPressRFeature _activeFeature;
        private readonly Input _input;

        public PressRMapComponent(Map map)
            : base(map)
        {
            GraphicsManager = new GraphicsManager();
            _input = new Input();

            DirectHaul = new DirectHaul(GraphicsManager, _input);
            DirectHaulStorageMode = new DirectHaulStorageMode(GraphicsManager, _input);
            StorageLens = new StorageLens(GraphicsManager, _input);

            _features.Add(StorageLens);
            _features.Add(DirectHaulStorageMode);
            _features.Add(DirectHaul);
        }

        public override void MapComponentOnGUI()
        {
            if (!IsActive)
            {
                return;
            }

            foreach (var feature in _features)
            {
                feature.ConstantUpdate();
            }

            if (!_input.IsPressRModifierKeyPressed)
            {
                if (_activeFeature != null)
                {
                    _activeFeature.Deactivate();
                    _activeFeature = null;
                    _input.Reset();
                }
                return;
            }

            _input.OnGUI();

            if (_activeFeature == null)
            {
                foreach (var feature in _features)
                {
                    if (feature.CanActivate())
                    {
                        _activeFeature = feature;
                        _activeFeature.Activate();
                        break;
                    }
                }
            }

            if (_activeFeature != null)
            {
                if (_activeFeature.CanActivate())
                {
                    _activeFeature.Update();
                }
                else
                {
                    _activeFeature.Deactivate();
                    _activeFeature = null;
                    _input.Reset();
                }
            }
        }

        public override void MapComponentUpdate()
        {
            bool shouldBeActive = Find.CurrentMap == map && !WorldRendererUtility.WorldSelected;

            if (IsActive && !shouldBeActive)
            {
                if (_activeFeature != null)
                {
                    _activeFeature.Deactivate();
                    _activeFeature = null;
                    _input.Reset();
                }
                foreach (var feature in _features)
                {
                    feature.ConstantClear();
                }
                IsActive = false;
                return;
            }

            if (!IsActive && shouldBeActive)
            {
                IsActive = true;
            }
            else if (!shouldBeActive)
            {
                return;
            }
            GraphicsManager?.UpdateTweens();
            GraphicsManager?.UpdateGraphicObjects();
            GraphicsManager?.RenderGraphicObjects();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe.EnterNode("DirectHaul");
            DirectHaul.ExposeData();
            Scribe.ExitNode();
        }
    }
}
