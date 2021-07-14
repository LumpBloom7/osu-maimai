using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Difficulty;
using osu.Game.Rulesets.Sentakki.Edit;
using osu.Game.Rulesets.Sentakki.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Statistics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki
{
    public class SentakkiRuleset : Ruleset
    {
        private static readonly Lazy<bool> is_development_build = new Lazy<bool>(() => typeof(SentakkiRuleset).Assembly.GetName().Name.EndsWith("-dev"));
        public static bool IsDevelopmentBuild => is_development_build.Value;

        public override string Description => IsDevelopmentBuild ? "sentakki (Dev build)" : "sentakki";
        public override string PlayingVerb => "Washing laundry";
        public override string ShortName => IsDevelopmentBuild ? "Sentakki-dev" : "Sentakki";

        public override ScoreProcessor CreateScoreProcessor() => new SentakkiScoreProcessor();

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableSentakkiRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new SentakkiBeatmapConverter(beatmap, this);

        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) =>
            new SentakkiBeatmapProcessor(beatmap);

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) =>
            new SentakkiDifficultyCalculator(this, beatmap);

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new SentakkiReplayFrame();

        public override HitObjectComposer CreateHitObjectComposer() => new SentakkiHitObjectComposer(this);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new MultiMod(new SentakkiModHalfTime(), new SentakkiModDaycore()),
                        new SentakkiModNoFail(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new SentakkiModHardRock(),
                        new MultiMod(new SentakkiModSuddenDeath(), new SentakkiModPerfect()),
                        new SentakkiModChallenge(),
                        new MultiMod(new SentakkiModDoubleTime(), new SentakkiModNightcore()),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new SentakkiModAutoplay(),
                        new SentakkiModAutoTouch()
                    };

                case ModType.Conversion:
                    return new Mod[]{
                        new SentakkiModMirror(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new SentakkiModSpin(),
                        new SentakkiModExperimental(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override RulesetSettingsSubsection CreateSettings() => new SentakkiSettingsSubsection(this);

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new SentakkiRulesetConfigManager(settings, RulesetInfo);

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, SentakkiAction.Button1),
            new KeyBinding(InputKey.X, SentakkiAction.Button2),
            new KeyBinding(InputKey.MouseLeft, SentakkiAction.Button1),
            new KeyBinding(InputKey.MouseRight, SentakkiAction.Button2),
            new KeyBinding(InputKey.Number1, SentakkiAction.Key1),
            new KeyBinding(InputKey.Number2, SentakkiAction.Key2),
            new KeyBinding(InputKey.Number3, SentakkiAction.Key3),
            new KeyBinding(InputKey.Number4, SentakkiAction.Key4),
            new KeyBinding(InputKey.Number5, SentakkiAction.Key5),
            new KeyBinding(InputKey.Number6, SentakkiAction.Key6),
            new KeyBinding(InputKey.Number7, SentakkiAction.Key7),
            new KeyBinding(InputKey.Number8, SentakkiAction.Key8),
        };

        public override StatisticRow[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => new[]
        {
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem("Timing Distribution", new HitEventTimingDistributionGraph(score.HitEvents)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 250
                    })
                }
            },
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem("Judgement Distribution", new JudgementChart(score.HitEvents.Where(e=>e.HitObject is SentakkiHitObject).ToList())
                    {
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(1, 250)
                    }),
                }
            },
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem(string.Empty, new SimpleStatisticTable(3, new SimpleStatisticItem[]
                    {
                        new UnstableRate(score.HitEvents)
                    }))
                }
            }
        };

        public override Drawable CreateIcon() => new SentakkiIcon(this);

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return new[]
            {
                HitResult.Great,
                HitResult.Good,
                HitResult.Ok,
            };
        }

        public override string GetDisplayNameForHitResult(HitResult result) => result.GetDisplayNameForSentakkiResult();

        public class SentakkiIcon : CompositeDrawable
        {
            private readonly Ruleset ruleset;

            public SentakkiIcon(Ruleset ruleset)
            {
                Anchor = Origin = Anchor.Centre;
                this.ruleset = ruleset;
                FillAspectRatio = 1;
                FillMode = FillMode.Fit;
                Size = new Vector2(100, 100);
            }

            // We don't want to generate a new texture store everytime this used, so we create a single texture store for all usages of this icon.
            private static LargeTextureStore textureStore;

            [BackgroundDependencyLoader]
            private void load(GameHost host)
            {
                if (textureStore is null)
                    textureStore = new LargeTextureStore(host.CreateTextureLoaderStore(ruleset.CreateResourceStore()));

                AddInternal(new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Texture = textureStore.Get("Textures/SentakkiIcon.png"),
                });

                if (IsDevelopmentBuild)
                {
                    AddInternal(new Container
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Size = new Vector2(60, 35),
                        Children = new Drawable[]{
                            // Used to offset the fonts being misaligned
                            new Container{
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Size = new Vector2(60,32),
                                CornerRadius = 8f,
                                CornerExponent = 2.5f,
                                Masking = true,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            },
                            new SpriteText
                            {
                                Text = "DEV",
                                Colour = Color4.Gray,
                                Font = OsuFont.Torus.With(size: 32,weight: FontWeight.Bold),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        }
                    });
                }
            }
        }
    }
}
