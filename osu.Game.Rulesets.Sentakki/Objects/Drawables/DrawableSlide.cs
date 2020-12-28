using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlide : DrawableSentakkiHitObject
    {
        public override bool DisplayResult => false;

        public Container<DrawableSlideBody> SlideBodies;
        public Container<DrawableSlideTap> SlideTaps;

        public DrawableSlide() : this(null) { }

        public DrawableSlide(SentakkiHitObject hitObject = null)
            : base(hitObject) { }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[]
            {
                SlideBodies = new Container<DrawableSlideBody>{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                SlideTaps = new Container<DrawableSlideTap>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // We also make sure all transforms have finished to avoid jank
            if (NestedHitObjects.All(n => n.Result.HasResult && Time.Current >= n.LatestTransformEndTime))
                ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SlideTap x:
                    return new DrawableSlideTap(x)
                    {
                        AutoBindable = { BindTarget = AutoBindable },
                    };
                case SlideBody slideBody:
                    return new DrawableSlideBody(slideBody)
                    {
                        AutoBindable = { BindTarget = AutoBindable },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlideBody node:
                    SlideBodies.Add(node);
                    break;
                case DrawableSlideTap tap:
                    SlideTaps.Child = tap;
                    break;
            }
            base.AddNestedHitObject(hitObject);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideBodies.Clear(false);
            SlideTaps.Clear(false);
        }
    }
}
