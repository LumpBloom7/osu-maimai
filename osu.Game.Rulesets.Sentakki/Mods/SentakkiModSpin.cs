﻿using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModSpin : Mod, IUpdatableByPlayfield
    {
        public override string Name => "Spin";
        public override string Description => "Replicate the true washing machine experience.";
        public override string Acronym => "S";

        public override IconUsage? Icon => FontAwesome.Solid.RedoAlt;
        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 1.00;

        [SettingSource("Revolution Duration", "The duration in seconds to complete a revolution")]
        public BindableNumber<int> RevolutionDuration { get; } = new BindableNumber<int>
        {
            MinValue = 3,
            MaxValue = 10,
            Default = 5,
            Value = 5
        };

        public void Update(Playfield playfield)
        {
            // We only rotate the main playfield
            if (playfield is SentakkiPlayfield)
                playfield.Rotation = (float)(playfield.Time.Current / (RevolutionDuration.Value * 1000)) * 360f;
        }
    }
}
