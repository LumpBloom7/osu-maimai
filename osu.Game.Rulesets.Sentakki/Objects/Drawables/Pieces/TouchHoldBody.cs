using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TouchHoldBody : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        private readonly TouchHoldCentrePiece centrePiece;

        private readonly HitExplosion explosion;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => centrePiece.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody()
        {
            Size = new Vector2(110);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]{
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece(),
                explosion = new HitExplosion(){
                    Size = new Vector2(110)
                },
            };
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader(true)]
        private void load(DrawableHitObject drawableObject)
        {
            if (drawableObject is null) return;

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explosion.Colour = colour.NewValue;
            }, true);

            drawableObject.ApplyCustomUpdateState += updateState;
        }

        private void updateState(DrawableHitObject drawableObject, ArmedState state)
        {
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime, true))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        explosion.Explode();

                        //after the flash, we can hide some elements that were behind it
                        ProgressPiece.FadeOut();
                        centrePiece.FadeOut();
                        break;
                }
            }
        }
    }
}
