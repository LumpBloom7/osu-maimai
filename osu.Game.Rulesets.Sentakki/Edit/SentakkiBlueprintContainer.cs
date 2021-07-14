using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiBlueprintContainer : ComposeBlueprintContainer
    {
        public SentakkiBlueprintContainer(HitObjectComposer composer)
            : base(composer)
        {
        }
        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SentakkiSelectionHandler();

        public override HitObjectSelectionBlueprint CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Tap t:
                    return new TapSelectionBlueprint(t);
                case Hold h:
                    return new HoldSelectionBlueprint(h);
                case Slide s:
                    return new SlideSelectionBlueprint(s);
                case Touch t:
                    return new TouchSelectionBlueprint(t);
                case TouchHold th:
                    return new TouchHoldSelectionBlueprint(th);
            }
            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        private Framework.Input.InputManager inputManager;
        internal Framework.Input.InputManager InputManager => inputManager ??= GetContainingInputManager();

        private Vector2 currentMousePosition => InputManager.CurrentState.Mouse.Position;

        protected override IEnumerable<SelectionBlueprint<HitObject>> SortForMovement(IReadOnlyList<SelectionBlueprint<HitObject>> blueprints)
            => blueprints.OrderBy(b => Vector2.DistanceSquared(b.ScreenSpaceSelectionPoint, currentMousePosition));
    }
}
