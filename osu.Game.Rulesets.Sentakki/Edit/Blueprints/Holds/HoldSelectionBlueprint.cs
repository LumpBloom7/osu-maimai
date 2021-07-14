using System;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds
{
    public class HoldSelectionBlueprint : SentakkiSelectionBlueprint<Hold>
    {
        public new DrawableHold DrawableObject => (DrawableHold)base.DrawableObject;

        private readonly HoldHighlight highlight;

        public HoldSelectionBlueprint(Hold hitObject)
            : base(hitObject)
        {
            InternalChild = highlight = new HoldHighlight();
        }

        protected override void Update()
        {
            base.Update();

            highlight.Rotation = DrawableObject.HitObject.Lane.GetRotationForLane();
            highlight.Note.Y = Math.Max(DrawableObject.NoteBody.Y, -SentakkiPlayfield.INTERSECTDISTANCE);
            highlight.Note.Height = DrawableObject.NoteBody.Height;
            highlight.Note.Scale = DrawableObject.NoteBody.Scale;
        }

        public override Vector2 ScreenSpaceSelectionPoint => highlight.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => highlight.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => highlight.ScreenSpaceDrawQuad;
    }
}
