using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Touch : SentakkiHitObject
    {
        public override Color4 DefaultNoteColour => Color4.Aqua;

        public Bindable<Vector2> PositionBindable = new Bindable<Vector2>();

        public override Vector2 Position
        {
            get => PositionBindable.Value;
            set => PositionBindable.Value = value;
        }

        protected override HitWindows CreateHitWindows() => new SentakkiTouchHitWindows();
    }
}
