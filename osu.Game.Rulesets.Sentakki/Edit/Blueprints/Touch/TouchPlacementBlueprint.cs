using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches
{
    public class TouchPlacementBlueprint : PlacementBlueprint
    {
        private readonly TouchHighlight highlight;

        public new Touch HitObject => (Touch)base.HitObject;

        public TouchPlacementBlueprint()
            : base(new Touch())
        {
            Anchor = Origin = Anchor.Centre;
            InternalChild = highlight = new TouchHighlight();
        }

        protected override void Update()
        {
            highlight.Position = HitObject.Position;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            var newPosition = ToLocalSpace(result.ScreenSpacePosition) - OriginPosition;

            if (Vector2.Distance(Vector2.Zero, newPosition) > 250)
            {
                var angle = Vector2.Zero.GetDegreesFromPosition(newPosition);
                newPosition = SentakkiExtensions.GetCircularPosition(250, angle);
            }
            HitObject.Position = newPosition;
        }
    }
}
