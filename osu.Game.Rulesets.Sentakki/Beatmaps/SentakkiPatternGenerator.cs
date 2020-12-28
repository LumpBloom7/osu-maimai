using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiPatternGenerator
    {
        public Bindable<ConversionExperiments> Experiments = new Bindable<ConversionExperiments>();
        private readonly Random rng;

        private IBeatmap beatmap;
        public SentakkiPatternGenerator(IBeatmap beatmap)
        {
            this.beatmap = beatmap;
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            int seed = ((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20) + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            rng = new Random(seed);
        }

        //The patterns will generate the note lane to be used based on the current offset
        // argument list is (offset, diff)
        private List<Func<bool, int>> patternlist => new List<Func<bool, int>>{
            //Stream pattern, lane difference determined by offset2
            (twin)=> {
                if(twin) return offset + 4;
                else offset+=offset2;
                return offset;
            },
            // Back and forth, works better with longer combos
            // Lane difference determined by offset2, but will make sure offset2 is never 0.
            (twin)=>{
                offset2 = offset2 == 0 ? 1 : offset2;
                offset+=offset2;
                offset2= -offset2;

                return offset;
            }
        };
        private int currentPattern;
        private int offset;
        private int offset2;

        private int getNewLane(bool twin = false) => patternlist[currentPattern].Invoke(twin).NormalizePath();

        public void CreateNewPattern()
        {
            currentPattern = rng.Next(0, patternlist.Count); // Pick a pattern
            offset = rng.Next(0, 8); // Give it a random offset for variety
            offset2 = rng.Next(-2, 3); // Give it a random offset for variety
        }

        public IEnumerable<SentakkiHitObject> GenerateNewNote(HitObject original)
        {
            bool breakNote = false;
            switch (original)
            {
                case IHasPathWithRepeats hold:
                    breakNote = hold.NodeSamples.Any(samples => samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH));
                    if (hold.NodeSamples.Any(samples => samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE)) && hold.Duration >= 350)
                    {
                        List<Slide> slides = new List<Slide>();
                        if (Experiments.Value.HasFlag(ConversionExperiments.twinSlides))
                        {
                            if (hold.NodeSamples.Any(samples => samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP)))
                            {
                                slides.Add((Slide)createSlideNote(original, true, breakNote));
                            }
                            else
                                foreach (var note in createTapsFromTicks(original).ToList())
                                    yield return note;
                        }
                        slides.Add((Slide)createSlideNote(original, isBreak: breakNote));

                        // Clean up potential duplicates
                        if (slides.Count >= 2)
                        {
                            // Merge if lanes are identical
                            if (slides.First().Lane == slides.Last().Lane)
                            {
                                // Make sure that both slides patterns are unique
                                if (!slides.First().SlideInfoList.Exists(x => x.ID == slides.Last().SlideInfoList.First().ID))
                                    slides.First().SlideInfoList.AddRange(slides.Last().SlideInfoList);

                                slides.Remove(slides.Last());
                            }
                        }
                        foreach (var note in slides)
                            yield return note;

                        yield break;
                    }
                    if (Experiments.Value.HasFlag(ConversionExperiments.twinNotes))
                    {
                        if (hold.NodeSamples.Any(samples => samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP)))
                        {
                            yield return createHoldNote(original, true, breakNote);
                        }
                        else
                            foreach (var note in createTapsFromTicks(original).ToList())
                                yield return note;
                    }

                    yield return createHoldNote(original, isBreak: breakNote);
                    break;

                case IHasDuration th:
                    if (th.Duration >= 100)
                        yield return CreateTouchHold(original);
                    break;

                default:
                    breakNote = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);
                    if (!breakNote && Experiments.Value.HasFlag(ConversionExperiments.touch) && original.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE))
                    {
                        yield return createTouchNote(original);
                    }
                    else
                    {
                        if (Experiments.Value.HasFlag(ConversionExperiments.twinNotes) && original.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP))
                        {
                            yield return createTapNote(original, true, breakNote);
                        }
                        yield return createTapNote(original, isBreak: breakNote);
                    }
                    break;
            }
        }

        // Individual note generation code, because it's cleaner
        public static SentakkiHitObject CreateTouchHold(HitObject original) => new TouchHold
        {
            StartTime = original.StartTime,
            EndTime = (original as IHasDuration).EndTime,
            Samples = original.Samples,
        };

        private SentakkiHitObject createSlideNote(HitObject original, bool twin = false, bool isBreak = false)
        {
            int noteLane = getNewLane(twin);

            var validPaths = SlidePaths.VALIDPATHS.Where(p => ((IHasDuration)original).Duration >= p.MinDuration && ((IHasDuration)original).Duration <= p.MaxDuration).ToList();
            if (!validPaths.Any()) return null;
            int selectedSlideID = SlidePaths.VALIDPATHS.IndexOf(validPaths[rng.Next(validPaths.Count)]);

            return new Slide
            {
                SlideInfoList = new List<SentakkiSlideInfo>{
                    new SentakkiSlideInfo{
                        ID = selectedSlideID,
                        Duration = ((IHasDuration)original).Duration
                    }
                },
                Lane = noteLane,
                StartTime = original.StartTime,
                NodeSamples = (original as IHasPathWithRepeats).NodeSamples,
                Break = isBreak
            };
        }

        private SentakkiHitObject createHoldNote(HitObject original, bool twin = false, bool isBreak = false)
        {
            int noteLane = getNewLane(twin);
            return new Hold
            {
                Break = isBreak,
                Lane = noteLane,
                NodeSamples = (original as IHasPathWithRepeats).NodeSamples,
                StartTime = original.StartTime,
                EndTime = original.GetEndTime()
            };
        }
        private IEnumerable<SentakkiHitObject> createTapsFromTicks(HitObject original)
        {
            int noteLane = getNewLane(true);

            var curve = original as IHasPathWithRepeats;
            double spanDuration = curve.Duration / (curve.RepeatCount + 1);
            bool isRepeatSpam = spanDuration < 75 && curve.RepeatCount > 0;

            if (isRepeatSpam)
                yield break;

            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;

            var controlPointInfo = beatmap.ControlPointInfo;
            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(original.StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(original.StartTime);

            double scoringDistance = 100 * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            var velocity = scoringDistance / timingPoint.BeatLength;
            var tickDistance = scoringDistance / difficulty.SliderTickRate;

            double legacyLastTickOffset = (original as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0;

            foreach (var e in SliderEventGenerator.Generate(original.StartTime, spanDuration, velocity, tickDistance, curve.Path.Distance, curve.RepeatCount + 1, legacyLastTickOffset, CancellationToken.None))
            {
                switch (e.Type)
                {
                    case SliderEventType.Tick:
                    case SliderEventType.Repeat:
                        yield return new Tap
                        {
                            Lane = noteLane,
                            Samples = original.Samples.Select(s => new HitSampleInfo(@"slidertick", s.Bank, s.Suffix, s.Volume)).ToList(),
                            StartTime = e.Time
                        };
                        break;
                }
            }
        }

        private SentakkiHitObject createTapNote(HitObject original, bool twin = false, bool isBreak = false)
        {
            int noteLane = getNewLane(twin);
            return new Tap
            {
                Break = isBreak,
                Lane = noteLane,
                Samples = original.Samples,
                StartTime = original.StartTime,
            };
        }

        private SentakkiHitObject createTouchNote(HitObject original)
        {
            return new Touch
            {
                Samples = original.Samples,
                StartTime = original.StartTime,
                Position = SentakkiExtensions.GetCircularPosition(rng.Next(200), rng.Next(360))
            };
        }
    }
}
