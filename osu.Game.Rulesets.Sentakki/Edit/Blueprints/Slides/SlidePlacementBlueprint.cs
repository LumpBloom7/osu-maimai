using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlidePlacementBlueprint : PlacementBlueprint
    {
        private readonly SlideStarHighlight highlight;

        public new Slide HitObject => (Slide)base.HitObject;
        public SlidePlacementBlueprint()
            : base(new Slide()
            {
                SlideInfoList = new List<SentakkiSlideInfo>{
                    new SentakkiSlideInfo{ ID = 13 }
                }
            })
        {
            Anchor = Origin = Anchor.Centre;
            InternalChild = highlight = new SlideStarHighlight();
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
