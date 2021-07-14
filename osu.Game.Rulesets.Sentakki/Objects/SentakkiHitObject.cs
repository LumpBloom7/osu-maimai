using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public abstract class SentakkiHitObject : HitObject, IHasPosition
    {
        // TODO:
        // No-op IHasPosition properties are added to allow work on editor. Remove ASAP

        public virtual Vector2 Position { get; set; }
        public float X => Position.X;
        public float Y => Position.Y;

        public SentakkiHitObject()
        {
            // We initialize the note colour to the default value first for test scenes
            // The colours during gameplay will be set during beatmap post-process
            ColourBindable.Value = DefaultNoteColour;
        }

        public override Judgement CreateJudgement() => new SentakkiJudgement();

        [JsonIgnore]
        public Bindable<Color4> ColourBindable = new Bindable<Color4>();

        [JsonIgnore]
        public Color4 NoteColour
        {
            get => ColourBindable.Value;
            set => ColourBindable.Value = value;
        }

        [JsonIgnore]
        public virtual Color4 DefaultNoteColour => Color4Extensions.FromHex("FF0064");

        protected override HitWindows CreateHitWindows() => new SentakkiHitWindows();

        // This special hitsample is used for Sentakki specific samples, with doesn't have bank specific variants
        public class SentakkiHitSampleInfo : HitSampleInfo, IEquatable<SentakkiHitSampleInfo>
        {
            public SentakkiHitSampleInfo(string name, int volume = 0) : base(name, volume: volume) { }
            public override IEnumerable<string> LookupNames
            {
                get
                {
                    yield return "Gameplay/" + Name;
                }
            }

#nullable enable
            public override HitSampleInfo With(Optional<string> newName = default, Optional<string?> newBank = default, Optional<string?> newSuffix = default, Optional<int> newVolume = default)
            {
                return new SentakkiHitSampleInfo(newName.GetOr(Name), newVolume.GetOr(Volume));
            }
#nullable disable

            public bool Equals(SentakkiHitSampleInfo other)
            {
                return other != null && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is SentakkiHitSampleInfo s && Equals(s);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }
    }
}
