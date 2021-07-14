using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTap : DrawableTap
    {
        protected override Drawable CreateTapRepresentation() => new SlideTapPiece();

        private DrawableSlide parentSlide => (DrawableSlide)ParentHitObject;

        public DrawableSlideTap() : this(null) { }
        public DrawableSlideTap(SlideTap hitObject)
            : base(hitObject) { }

        protected override void OnApply()
        {
            base.OnApply();
            AccentColour.BindTo(ParentHitObject.AccentColour);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(parentSlide.AccentColour);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            var note = TapVisual as SlideTapPiece;

            if (parentSlide.SlideBodies.Count > 1)
                note.SecondStar.Alpha = 1;
            else
                note.SecondStar.Alpha = 0;

            double spinDuration = ((Slide)parentSlide.HitObject).SlideInfoList.FirstOrDefault().Duration;
            if (spinDuration != 0)
                note.Stars.Spin(spinDuration, RotationDirection.Counterclockwise, 0).Loop();
        }
    }
}
