using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests
{
    [TestFixture]
    public class TestSceneSentakkiEditor : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new SentakkiRuleset();
    }
}
