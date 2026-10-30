using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LegacyThroughTime.Prototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LegacyThroughTime.PrototypeTests
{
    /// The chat with the hero of a story on a real canvas, without the network: the replies are given to the screen.
    /// A message costs energy, no energy shuts the field and shows the card, a lost reply gives everything back.
    public sealed class ChatWalkThrough
    {
        static void Click(GameObject go) => ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        static readonly List<GameObject> made = new();     // the canvases of the tests; the scene's own (the story player's) stay

        static RectTransform Canvas()
        {
            LegacyThroughTime.Prototype.Stage.Setup();
            var canvas = LegacyThroughTime.Prototype.Stage.BuildCanvas();
            made.Add(canvas.gameObject);
            return canvas;
        }

        sealed class Setup
        {
            public ChatScreen Screen;
            public Chat Chat;
            public Energy Energy;
            public int Shop, Left;
            public readonly List<string> Told = new();
            public RectTransform Canvas;

            public InputField Field => Screen.Root.GetComponentsInChildren<InputField>().FirstOrDefault(f => f);
            public PressButton Send => Screen.Root.GetComponentsInChildren<PressButton>().FirstOrDefault(b => b.name == "Send");
            public bool Has(string name) => Screen.Root.GetComponentsInChildren<Transform>().Any(t => t.name == name);
        }

        static Setup Open(int energy)
        {
            var s = new Setup { Canvas = Canvas() };
            var story = StoryCatalog.All[0];
            s.Chat = Chat.Begin(story.Id, story.Opening, 1);
            s.Energy = new Energy { value = energy };
            s.Screen = new ChatScreen(s.Canvas, null, story, s.Chat, s.Energy, null, () => s.Left++, () => s.Shop++, (t, _) => s.Told.Add(t), animated: false);
            return s;
        }

        [TearDown]
        public void Clean()
        {
            foreach (var go in made) if (go) Object.Destroy(go);
            made.Clear();
            PlayerPrefs.DeleteKey("chat:" + StoryCatalog.All[0].Id);
            PlayerPrefs.DeleteKey("chat:energy");
        }

        [UnityTest]
        public IEnumerator TheChatOpensWithTheAuthorsLineAndAnEmptyField()
        {
            var s = Open(energy: 5);
            yield return null;
            Assert.That(s.Screen.Count, Is.EqualTo(2), "the note and the author's first line");
            Assert.That(s.Send.Interactable, Is.False, "nothing to send");
            Assert.That(s.Has("Limit"), Is.False);
            Assert.That(s.Field.interactable, Is.True);
            UnityEngine.Canvas.ForceUpdateCanvases();
            var note = s.Screen.Root.GetComponentsInChildren<Text>().First(t => t.name == "Text" && t.text.StartsWith("Диалог вымышленный"));
            Assert.That(note.rectTransform.rect.width, Is.LessThanOrEqualTo(Metrics.RefWidth), "the note wraps inside the screen");
        }

        [UnityTest]
        public IEnumerator AMessageCostsEnergyAndTheReplyComesAfterTyping()
        {
            var s = Open(energy: 5);
            yield return null;
            s.Field.text = "Привет";
            Assert.That(s.Send.Interactable, Is.True);
            Click(s.Send.gameObject);
            yield return null;

            Assert.That(s.Energy.value, Is.EqualTo(4));
            Assert.That(s.Chat.lines.Select(l => l.role), Is.EqualTo(new[] { ChatLine.Hero, ChatLine.Reader }));
            Assert.That(s.Screen.Busy, Is.True);
            Assert.That(s.Has("Typing"), Is.True, "the three dots");
            Assert.That(s.Send.Interactable, Is.False, "one message at a time");
            Assert.That(s.Field.text, Is.Empty);

            s.Screen.Deliver("Привет, путник.");
            yield return null;
            Assert.That(s.Screen.Busy, Is.False);
            Assert.That(s.Has("Typing"), Is.False);
            Assert.That(s.Chat.Last.text, Is.EqualTo("Привет, путник."));
            Assert.That(s.Chat.Last.role, Is.EqualTo(ChatLine.Hero));
        }

        [UnityTest]
        public IEnumerator NoEnergyShutsTheFieldAndShowsTheCardThatLeadsToTheShop()
        {
            var s = Open(energy: 1);
            yield return null;
            s.Field.text = "Ты здесь?";
            Click(s.Send.gameObject);
            yield return null;
            s.Screen.Deliver("Здесь.");
            yield return null;

            Assert.That(s.Energy.value, Is.EqualTo(0));
            Assert.That(s.Has("Limit"), Is.True, "«Энергия закончилась»");
            Assert.That(s.Field.interactable, Is.False);
            s.Field.text = "ещё";
            Assert.That(s.Send.Interactable, Is.False, "words are there but no energy");
            Click(s.Screen.Root.GetComponentsInChildren<PressButton>().First(b => b.name == "Refill").gameObject);
            Assert.That(s.Shop, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ALostReplyGivesTheWordsAndTheEnergyBack()
        {
            var s = Open(energy: 3);
            yield return null;
            s.Field.text = "Расскажи про степь";
            Click(s.Send.gameObject);
            yield return null;
            s.Screen.Fail("Нет связи. Проверьте интернет");
            yield return null;

            Assert.That(s.Energy.value, Is.EqualTo(3));
            Assert.That(s.Chat.lines.Count, Is.EqualTo(1), "only the author's line");
            Assert.That(s.Has("Typing"), Is.False);
            Assert.That(s.Field.text, Is.EqualTo("Расскажи про степь"), "the words come back into the field");
            Assert.That(s.Told, Is.EqualTo(new[] { "Нет связи. Проверьте интернет" }));
        }

        [UnityTest]
        public IEnumerator TheEmptyEnergyIsThereFromTheStart()
        {
            var s = Open(energy: 0);
            yield return null;
            Assert.That(s.Has("Limit"), Is.True);
            Assert.That(s.Field.interactable, Is.False);
        }

        [UnityTest]
        public IEnumerator TheArrowAtTheTopLeavesTheChatOnce()
        {
            var s = Open(energy: 5);
            yield return null;
            var back = s.Screen.Root.GetComponentsInChildren<PressButton>().First(b => b.name == "BackHit");
            Click(back.gameObject); Click(back.gameObject);
            Assert.That(s.Left, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TheListOfChatsHoldsTheHeroOfAStartedStory()
        {
            var canvas = Canvas();
            var story = StoryCatalog.All[0];
            var rows = ChatList.Build(StoryCatalog.All, _ => true, _ => Chat.Begin(story.Id, "Привет.", 1));
            StoryEntry opened = null; var toStories = 0;
            var panel = new ChatsPanel(canvas, rows, e => opened = e, () => toStories++, System.DateTime.Now, NoAnimator.Instance);
            yield return null;
            Assert.That(panel.Rows, Is.EqualTo(1), "announced stories have no chat");
            Click(panel.Root.GetComponentsInChildren<PressButton>().First(b => b.name.StartsWith("Chat ")).gameObject);
            Assert.That(opened, Is.SameAs(story));

            var empty = new ChatsPanel(canvas, new List<ChatRow>(), _ => { }, () => toStories++, System.DateTime.Now, NoAnimator.Instance);
            yield return null;
            Click(empty.Root.GetComponentsInChildren<PressButton>().First(b => b.name == "ToStories").gameObject);
            Assert.That(toStories, Is.EqualTo(1));
        }
    }
}
