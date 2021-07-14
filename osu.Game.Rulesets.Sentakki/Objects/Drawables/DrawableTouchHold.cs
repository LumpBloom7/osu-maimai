using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouchHold : DrawableSentakkiHitObject
    {
        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => TouchHoldBody.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody TouchHoldBody;

        private PausableSkinnableSound holdSample;

        public DrawableTouchHold() : this(null) { }

        public DrawableTouchHold(TouchHold hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
            Colour = Color4.SlateGray;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0f);
            Alpha = 0;
            AddRangeInternal(new Drawable[] {
                TouchHoldBody = new TouchHoldBody(),
                holdSample = new PausableSkinnableSound
                {
                    Volume = { Value = 0 },
                    Looping = true,
                    Frequency = { Value = 1 }
                }
            });

            isHitting.BindValueChanged(b =>
            {
                if (b.NewValue) beginHold();
                else endHold();
            });
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            var firstSample = HitObject.Samples.FirstOrDefault();

            if (firstSample != null)
            {
                var clone = HitObject.SampleControlPoint.ApplyTo(firstSample).With("spinnerspin");

                holdSample.Samples = new ISampleInfo[] { clone };
                holdSample.Frequency.Value = 1;
            }
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            holdSample?.Stop();
        }

        [Resolved]
        private OsuColour colours { get; set; }

        protected override void OnFree()
        {
            base.OnFree();

            holdSample.Samples = null;
            isHitting.Value = false;
            totalHoldTime = 0;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double fadeIn = AdjustedAnimationDuration;
            this.FadeInFromZero(fadeIn).ScaleTo(1, fadeIn);
            using (BeginDelayedSequence(fadeIn, true))
            {
                TouchHoldBody.ProgressPiece.TransformBindableTo(TouchHoldBody.ProgressPiece.ProgressBindable, 1, ((IHasDuration)HitObject).Duration);
            }
        }

        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;
        private double totalHoldTime;

        private void beginHold()
        {
            holdStartTime = Math.Max(Time.Current, HitObject.StartTime);
            Colour = Color4.White;
            if (!holdSample.IsPlaying)
                holdSample.Play();
            holdSample.VolumeTo(1, 300);
        }

        private void endHold()
        {
            if (holdStartTime.HasValue)
                totalHoldTime += Math.Max(Time.Current - holdStartTime.Value, 0);

            holdStartTime = null;
            Colour = Color4.SlateGray;
            holdSample.VolumeTo(0, 150);
        }

        protected override void Update()
        {
            base.Update();

            isHitting.Value = Time.Current >= HitObject.StartTime
                            && Time.Current <= HitObject.GetEndTime()
                            && (Auto || checkForTouchInput() || ((SentakkiActionInputManager?.PressedActions.Any() ?? false) && IsHovered));

            if (holdSample != null && isHitting.Value)
                holdSample.Frequency.Value = 0.5 + ((Time.Current - holdStartTime.Value + totalHoldTime) / ((IHasDuration)HitObject).Duration);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < (HitObject as IHasDuration)?.EndTime) return;

            double result = totalHoldTime / (HitObject as IHasDuration).Duration;

            HitResult resultType;

            if (result >= .75)
                resultType = HitResult.Great;
            else if (result >= .5)
                resultType = HitResult.Good;
            else if (result >= .25)
                resultType = HitResult.Ok;
            else
                resultType = HitResult.Miss;

            AccentColour.Value = colours.ForHitResult(resultType);
            ApplyResult(resultType);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_hit = 100, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay(time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss).Expire();
                    break;
            }
        }

        private bool checkForTouchInput()
        {
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            // Avoiding Linq to minimize allocations, since this would be called every update of this node
            foreach (var t in touchInput.ActiveSources)
                if (ReceivePositionalInputAt(touchInput.GetTouchPosition(t).Value))
                    return true;

            return false;
        }
    }
}
