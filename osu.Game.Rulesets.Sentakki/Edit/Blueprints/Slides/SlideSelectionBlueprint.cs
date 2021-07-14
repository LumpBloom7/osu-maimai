using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlideSelectionBlueprint : SentakkiSelectionBlueprint<Slide>
    {
        public new DrawableSlide DrawableObject => (DrawableSlide)base.DrawableObject;

        private readonly SlideStarHighlight starHighlight;
        private readonly SlideBodyHighlight bodyHighlight;

        public SlideSelectionBlueprint(Slide hitObject)
            : base(hitObject)
        {
            InternalChildren = new Drawable[]{
                bodyHighlight = new SlideBodyHighlight(),
                starHighlight = new SlideStarHighlight()
            };
        }

        protected override void Update()
        {
            base.Update();

            var SlideTap = DrawableObject.SlideTaps.Child;

            var FirstSlideBody = DrawableObject.SlideBodies.FirstOrDefault();

            if (DrawableObject.Time.Current < DrawableObject.HitObject.StartTime)
            {
                starHighlight.Rotation = SlideTap.HitObject.Lane.GetRotationForLane();
                starHighlight.Note.Position = new Vector2(0, Math.Max(SlideTap.TapVisual.Y, -SentakkiPlayfield.INTERSECTDISTANCE));
                starHighlight.Note.Scale = SlideTap.TapVisual.Scale;
                starHighlight.Note.Rotation = (SlideTap.TapVisual as SlideTapPiece).Stars.Rotation;
            }
            else
            {
                if (FirstSlideBody is null)
                    return;

                starHighlight.Rotation = SlideTap.HitObject.Lane.GetRotationForLane() - 22.5f;
                starHighlight.Note.Position = FirstSlideBody.SlideStar.Position;
                starHighlight.Note.Scale = FirstSlideBody.SlideStar.Scale;
                starHighlight.Note.Rotation = FirstSlideBody.SlideStar.Rotation;
            }

            if (FirstSlideBody is null)
                return;

            bodyHighlight.Rotation = SlideTap.HitObject.Lane.GetRotationForLane() - 22.5f;
            bodyHighlight.Path.Vertices = DrawableObject.SlideBodies.First().HitObject.SlideInfo.SlidePath.Vertices;
        }

        public override Vector2 ScreenSpaceSelectionPoint => starHighlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            => starHighlight.ReceivePositionalInputAt(screenSpacePos) || bodyHighlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => starHighlight.ScreenSpaceDrawQuad;
    }
}
