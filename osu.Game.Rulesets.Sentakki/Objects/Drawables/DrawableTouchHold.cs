﻿using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouchHold : DrawableSentakkiHitObject
    {
        private readonly TouchHoldCircle circle;

        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        protected override double InitialLifetimeOffset => 4000;

        public DrawableTouchHold(TouchHold hitObject)
            : base(hitObject)
        {
            AccentColour.Value = Color4.HotPink;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(120);
            Scale = new Vector2(0f);
            RelativeSizeAxes = Axes.None;
            Alpha = 0;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[] {
                circle = new TouchHoldCircle(){ Duration = hitObject.Duration },
            });

            OnNewResult += (DrawableHitObject obj, JudgementResult result) =>
            {
                AccentColour.Value = colours.ForHitResult(result.Type);
            };
            OnRevertResult += (DrawableHitObject obj, JudgementResult result) =>
            {
                AccentColour.Value = Color4.HotPink;
            };
            activated.BindValueChanged(b =>
            {
                circle.FadeTo(b.NewValue ? 1 : .5f, 100);
                circle.ScaleTo(b.NewValue ? 1 : .8f, 100);
            });
        }

        private double timeHeld;

        private readonly BindableBool activated = new BindableBool(false);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasDuration)?.EndTime)
                return;

            double result = timeHeld / (HitObject as IHasDuration).Duration;

            ApplyResult(r =>
            {
                if (result >= .9)
                    r.Type = HitResult.Perfect;
                else if (result >= .75)
                    r.Type = HitResult.Great;
                else if (result >= .5)
                    r.Type = HitResult.Good;
                else if (result >= .25)
                    r.Type = HitResult.Ok;
                else if (Time.Current >= (HitObject as IHasDuration)?.EndTime)
                    r.Type = HitResult.Miss;
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
        }

        [Resolved]
        private OsuColour colours { get; set; }

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        protected override void InvalidateTransforms()
        {
        }

        protected override void UpdateInitialTransforms()
        {
            double fadeIn = AnimationDuration.Value * GameplaySpeed;
            using (BeginAbsoluteSequence(HitObject.StartTime - fadeIn, true))
            {
                this.FadeInFromZero(fadeIn).ScaleTo(1, fadeIn);
                using (BeginDelayedSequence(fadeIn, true))
                {
                    circle.Progress.FillTo(1, (HitObject as IHasDuration).Duration);
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            activated.Value = Time.Current >= HitObject.StartTime
                            && Time.Current <= (HitObject as IHasDuration)?.EndTime
                            && (Auto || ((SentakkiActionInputManager?.PressedActions.Any() ?? false) && IsHovered));

            if (Result.HasResult) return;

            // Input and feedback
            if (Time.Current >= HitObject.StartTime && Time.Current <= (HitObject as IHasDuration)?.EndTime)
            {
                if (activated.Value)
                {
                    double prevProg = timeHeld / (HitObject as IHasDuration).Duration;
                    timeHeld += Time.Elapsed;
                    double progress = timeHeld / (HitObject as IHasDuration).Duration;

                    if (progress >= .25f && prevProg < .25f)
                    {
                        circle.ResizeTo(1.033f, 100);
                        this.TransformBindableTo(AccentColour, colours.ForHitResult(HitResult.Meh), 100);
                    }
                    else if (progress >= .50f && prevProg < .50f)
                    {
                        circle.ResizeTo(1.066f, 100);
                        this.TransformBindableTo(AccentColour, colours.ForHitResult(HitResult.Good), 100);
                    }
                    else if (progress >= .75f && prevProg < .75f)
                    {
                        circle.ResizeTo(1.1f, 100);
                        this.TransformBindableTo(AccentColour, colours.ForHitResult(HitResult.Great), 100);
                    }
                }
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay((HitObject as IHasDuration).Duration).ScaleTo(1f, time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.Delay((HitObject as IHasDuration).Duration).ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss);
                    break;
            }
        }
    }
}
