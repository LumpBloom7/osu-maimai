using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlideStarHighlight : CompositeDrawable
    {
        public readonly Container Note;

        // This drawable is zero width
        // We should use the quad of the note container
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Note.ReceivePositionalInputAt(screenSpacePos);
        public override Quad ScreenSpaceDrawQuad => Note.ScreenSpaceDrawQuad;

        public SlideStarHighlight()
        {
            Anchor = Origin = Anchor.Centre;
            Colour = Color4.YellowGreen;
            Alpha = 0.5f;
            InternalChildren = new Drawable[]
            {
                Note = new Container{
                    Size = new Vector2(75),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        new StarPiece()
                    }
                }
            };
        }
    }
}
