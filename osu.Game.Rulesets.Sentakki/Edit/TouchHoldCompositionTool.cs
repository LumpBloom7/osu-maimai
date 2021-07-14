using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class TouchHoldCompositionTool : HitObjectCompositionTool
    {
        public TouchHoldCompositionTool()
            : base(nameof(TouchHold))
        {
        }

        public override Drawable CreateIcon() => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners);

        public override PlacementBlueprint CreatePlacementBlueprint() => new TouchHoldPlacementBlueprint();
    }
}
