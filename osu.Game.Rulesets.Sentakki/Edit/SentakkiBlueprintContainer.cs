using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiBlueprintContainer : ComposeBlueprintContainer
    {
        public SentakkiBlueprintContainer(IEnumerable<DrawableHitObject> drawableHitObjects)
            : base(drawableHitObjects)
        {
        }

        protected override SelectionHandler CreateSelectionHandler() => new SentakkiSelectionHandler();

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableTap tap:
                    return new TapSelectionBlueprint(tap);
                case DrawableHold hold:
                    return new HoldSelectionBlueprint(hold);
                case DrawableTouchHold touchHold:
                    return new TouchHoldSelectionBlueprint(touchHold);
            }

            return base.CreateBlueprintFor(hitObject);
        }
    }
}
