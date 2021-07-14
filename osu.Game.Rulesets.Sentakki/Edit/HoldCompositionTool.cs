using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class HoldCompositionTool : HitObjectCompositionTool
    {
        public HoldCompositionTool()
            : base(nameof(Hold))
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders);

        public override PlacementBlueprint CreatePlacementBlueprint() => new HoldPlacementBlueprint();
    }
}
