using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SlideCompositionTool : HitObjectCompositionTool
    {
        public SlideCompositionTool()
            : base(nameof(Slide))
        {
        }

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Regular.Star,
        };

        public override PlacementBlueprint CreatePlacementBlueprint() => new SlidePlacementBlueprint();
    }
}
