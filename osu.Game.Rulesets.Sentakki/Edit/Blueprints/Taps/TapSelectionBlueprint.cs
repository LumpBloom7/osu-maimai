using System;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps
{
    public class TapSelectionBlueprint : SentakkiSelectionBlueprint<Tap>
    {
        public new DrawableTap DrawableObject => (DrawableTap)base.DrawableObject;

        private readonly TapHighlight highlight;

        public TapSelectionBlueprint(Tap hitObject)
            : base(hitObject)
        {
            InternalChild = highlight = new TapHighlight();
        }

        protected override void Update()
        {
            base.Update();

            highlight.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
            highlight.Note.Y = Math.Max(DrawableObject.TapVisual.Y, -SentakkiPlayfield.INTERSECTDISTANCE);
            highlight.Note.Scale = DrawableObject.TapVisual.Scale;
        }

        public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
    }
}
