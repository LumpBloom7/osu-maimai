using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches
{
    public class TouchSelectionBlueprint : SentakkiSelectionBlueprint<Touch>
    {
        public new DrawableTouch DrawableObject => (DrawableTouch)base.DrawableObject;

        private readonly TouchHighlight highlight;

        public TouchSelectionBlueprint(Touch hitObject)
            : base(hitObject)
        {
            InternalChild = highlight = new TouchHighlight();
        }

        protected override void Update()
        {
            base.Update();

            highlight.Position = DrawableObject.Position;
            highlight.Size = DrawableObject.TouchBody.Size;
            highlight.BorderContainer.Alpha = DrawableObject.TouchBody.BorderContainer.Alpha;
        }

        public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
    }
}
