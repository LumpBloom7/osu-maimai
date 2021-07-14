using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public class TouchHoldPlacementBlueprint : PlacementBlueprint
    {
        public new TouchHold HitObject => (TouchHold)base.HitObject;

        public TouchHoldPlacementBlueprint()
            : base(new TouchHold())
        {
            Anchor = Origin = Anchor.Centre;
            InternalChild = new TouchHoldHighlight();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            BeginPlacement(true);

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Left)
                return;

            EndPlacement(true);
        }

        private double originalStartTime;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double endTime)
                {
                    HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                    HitObject.Duration = Math.Abs(endTime - originalStartTime);
                }
            }
            else
            {
                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
        }
    }
}
