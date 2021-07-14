using Microsoft.EntityFrameworkCore.Internal;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public class SlideBodyHighlight : CompositeDrawable
    {
        public readonly SlideEditPath Path;

        // This drawable is zero width
        // We should use the quad of the note container
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Path.ReceivePositionalInputAt(screenSpacePos);
        public override Quad ScreenSpaceDrawQuad => Path.ScreenSpaceDrawQuad;

        public SlideBodyHighlight()
        {
            Anchor = Origin = Anchor.Centre;
            Colour = Color4.YellowGreen;
            Alpha = 0.5f;
            InternalChildren = new Drawable[]
            {
                Path = new SlideEditPath()
            };
        }

        public class SlideEditPath : SmoothPath
        {
            public override Vector2 OriginPosition => Vertices.Any() ? PositionInBoundingBox(Vertices[0]) : base.OriginPosition;

            public SlideEditPath()
            {
                PathRadius = 25;
                Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            }

            protected override Color4 ColourAt(float position) => position <= .2f ? Color4.White : Color4.Transparent;
        }
    }
}
