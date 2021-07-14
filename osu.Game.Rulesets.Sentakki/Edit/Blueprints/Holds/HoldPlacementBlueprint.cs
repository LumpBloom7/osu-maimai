using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds
{
    public class HoldPlacementBlueprint : PlacementBlueprint
    {
        private readonly HoldHighlight highlight;

        public new Hold HitObject => (Hold)base.HitObject;

        public HoldPlacementBlueprint()
            : base(new Hold())
        {
            Anchor = Origin = Anchor.Centre;
            InternalChild = highlight = new HoldHighlight();
            highlight.Note.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
        }

        protected override void Update()
        {
            highlight.Rotation = HitObject.Lane.GetRotationForLane();
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
                HitObject.Lane = OriginPosition.GetDegreesFromPosition(ToLocalSpace(result.ScreenSpacePosition)).GetNoteLaneFromDegrees();
                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
        }
    }
}
