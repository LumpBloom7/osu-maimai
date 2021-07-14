using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TouchBody : Container
    {
        public Container BorderContainer;
        public Container PieceContainer;
        public TouchBody()
        {
            Size = new Vector2(130);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]{
                BorderContainer = new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size= new Vector2(105),
                    CornerRadius = 25f,
                    CornerExponent = 2.5f,
                    Masking = true,
                    BorderThickness = 15,
                    BorderColour = Color4.White,
                    Alpha = 0,
                    Child = new Box{
                        Alpha = 0,
                        AlwaysPresent = true,
                        RelativeSizeAxes = Axes.Both
                    }
                },
                PieceContainer = new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes= Axes.Both,
                    Children = new Drawable[]{
                        new TouchPiece{
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        new TouchPiece{
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.TopCentre,
                            Rotation = 180
                        },
                        new TouchPiece{
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.TopCentre,
                            Rotation = 270
                        },
                        new TouchPiece{
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.TopCentre,
                            Rotation = 90
                        },
                        new DotPiece()
                    }
                }
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
                PieceContainer.Colour = colour.NewValue;
            }, true);
        }
    }
}
