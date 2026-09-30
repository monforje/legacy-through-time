using System.Linq;
using LegacyThroughTime.Narrative.Ink;
using NUnit.Framework;

namespace LegacyThroughTime.Narrative.Tests
{
    [TestFixture]
    public class StyleTests
    {
        static StoryMachine Machine(string ink, int seed = 1) => StoryMachine.FromJson(Corpus.CompileSource(ink), seed: seed);

        [Test]
        public void Dialogue_Narration_And_Voice_Tags()
        {
            var m = Machine(@"
Сафия: Ханбике.
Рассвет над степью. # narr
Он пахнет домом. # thought
НОВЫЙ ПАРАМЕТР: «ПАМЯТЬ СТЕПИ». # system
Маленькая Сююмбике: А если порву? # flashback
Копыта. # sfx
Просто текст без тегов.
Время 12:30 без спикера.
-> END");
            m.Advance(); Assert.AreEqual(("Сафия", "Ханбике.", Voice.Dialogue), (m.Current.Speaker, m.Current.Text, m.Current.Voice));
            m.Advance(); Assert.AreEqual(((string)null, "Рассвет над степью.", Voice.Narration), (m.Current.Speaker, m.Current.Text, m.Current.Voice));
            m.Advance(); Assert.AreEqual(((string)null, Voice.Thought), (m.Current.Speaker, m.Current.Voice));
            m.Advance(); Assert.AreEqual(((string)null, "НОВЫЙ ПАРАМЕТР: «ПАМЯТЬ СТЕПИ».", Voice.System), (m.Current.Speaker, m.Current.Text, m.Current.Voice));
            m.Advance(); Assert.AreEqual(("Маленькая Сююмбике", "А если порву?", Voice.Flashback), (m.Current.Speaker, m.Current.Text, m.Current.Voice));
            m.Advance(); Assert.AreEqual(Voice.Sound, m.Current.Voice);
            m.Advance(); Assert.AreEqual(((string)null, Voice.Narration), (m.Current.Speaker, m.Current.Voice));
            m.Advance(); Assert.AreEqual(((string)null, "Время 12:30 без спикера."), (m.Current.Speaker, m.Current.Text));
            m.Advance(); Assert.AreEqual(StoryPhase.End, m.Phase);
        }

        [Test]
        public void Tags_Above_A_Line_Belong_To_That_Line()
        {
            var m = Machine(@"
# bg: steppe_dawn
# music: kobyz
Text after.
-> END");
            m.Advance();
            Assert.AreEqual(StoryPhase.Line, m.Phase);
            Assert.AreEqual("Text after.", m.Current.Text);
            Assert.AreEqual("steppe_dawn", m.Current.TagValue("bg"));
            Assert.AreEqual("kobyz", m.Current.TagValue("music"));
        }

        [Test]
        public void Tags_With_No_Following_Text_Form_A_Directive()
        {
            var m = Machine(@"
Text.
# music: stop
* [A] -> END");
            m.Advance();
            Assert.AreEqual(("Text.", 0), (m.Current.Text, m.Current.Tags.Count));
            m.Advance();
            Assert.AreEqual(StoryPhase.Directive, m.Phase);
            Assert.AreEqual("stop", m.Current.TagValue("music"));
            m.Advance();
            Assert.AreEqual(StoryPhase.Choice, m.Phase);
        }

        [Test]
        public void Premium_Options_Expose_Price_And_Clean_Text()
        {
            var m = Machine(@"
VAR gems = 20
Выбор.
* [Сесть в повозку] -> END
* {gems >= 5} [💎 Взять с собой нож (5 💎)] -> END
* [Остаться # mood: calm] -> END");
            m.Advance();
            m.Advance();
            Assert.AreEqual(StoryPhase.Choice, m.Phase);
            var o = m.Current.Options;
            Assert.AreEqual(3, o.Count);
            Assert.IsFalse(o[0].IsPremium);
            Assert.IsTrue(o[1].IsPremium);
            Assert.AreEqual(5, o[1].Cost);
            Assert.AreEqual("Взять с собой нож", o[1].Text);
            Assert.AreEqual("💎 Взять с собой нож (5 💎)", o[1].RawText);
            Assert.AreEqual("calm", o[2].Tags.Single(t => t.Key == "mood").Value);
        }

        [Test]
        public void Beat_Reports_Variable_Delta()
        {
            var m = Machine(@"
VAR gems = 20
VAR steppe_memory = 0
Начало.
* [Надеть калфак]
    ~ steppe_memory++
    ПАМЯТЬ СТЕПИ +1 # system
    -> END
* [💎 Спрятать (8 💎)]
    ~ gems -= 8
    Спрятано.
    -> END");
            m.Advance();
            m.Advance();
            Assert.IsTrue(m.Choose(1));
            var d = m.Current.Changes.Single();
            Assert.AreEqual(("gems", 20, 12), (d.Name, d.Before, d.After));
            Assert.AreEqual(12, m.GetInt("gems"));
        }

        [Test]
        public void Runtime_Error_Moves_To_Absorbing_Fault()
        {
            var m = Machine(@"
VAR x = 0
Before.
~ x = 1 / x
After.
-> END");
            m.Advance();
            m.Advance();
            Assert.AreEqual(StoryPhase.Fault, m.Phase);
            StringAssert.Contains("Division by zero", m.Current.Error);
            Assert.IsFalse(m.Advance());
        }

        [Test]
        public void Host_Can_Write_Variables_And_Bind_Externals()
        {
            var m = Machine(@"
EXTERNAL play_sound(name)
VAR gems = 0
~ play_sound(""kobyz"")
Камней {gems}.
-> END
=== function play_sound(name) ===
~ return 0");
            string played = null;
            m.BindExternal("play_sound", args => { played = (string)args[0]; return null; });
            m.SetVariable("gems", 42);
            m.Advance();
            Assert.AreEqual("Камней 42.", m.Current.Text);
            Assert.AreEqual("kobyz", played);
        }
    }
}
