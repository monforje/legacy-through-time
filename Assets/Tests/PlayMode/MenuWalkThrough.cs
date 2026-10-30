using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LegacyThroughTime.Prototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LegacyThroughTime.PrototypeTests
{
    /// The shell's menu on a real canvas: a popup (the wheel, the rewards) never stays over the screen it opens.
    public sealed class MenuWalkThrough
    {
        static readonly List<GameObject> made = new();

        static MenuScreen Open()
        {
            LegacyThroughTime.Prototype.Stage.Setup();
            var canvas = LegacyThroughTime.Prototype.Stage.BuildCanvas();
            made.Add(canvas.gameObject);
            var column = Kit.Column(canvas);
            return new MenuScreen(column, StoryCatalog.All, c => MenuScreen.Progress.New, (_, _, _) => { }, _ => { }, new List<Glossary.Term>(), animated: false);
        }

        static bool Has(MenuScreen menu, string name) => menu.Root.parent.GetComponentsInChildren<Transform>().Any(t => t && t.name == name && t.gameObject.activeInHierarchy);

        [TearDown]
        public void Clean()
        {
            foreach (var go in made) if (go) Object.Destroy(go);
            made.Clear();
        }

        [UnityTest]
        public IEnumerator ThePlusOfTheWheelOpensTheShopInFrontAndBackComesToTheWheel()
        {
            var menu = Open();
            menu.Select(MenuScreen.Tab.Wheel);
            yield return null;
            Assert.That(Has(menu, "Dialog"), Is.True, "the wheel is open");

            menu.Select(MenuScreen.Tab.Shop);                  // what the "+" of the wheel does
            yield return null;
            Assert.That(Has(menu, "Dialog"), Is.False, "the wheel is gone, the shop is not under it");
            Assert.That(menu.Current, Is.EqualTo(MenuScreen.Tab.Shop));

            Assert.That(menu.Back(), Is.True);                 // Android Back in the shop
            yield return null;
            Assert.That(menu.Current, Is.EqualTo(MenuScreen.Tab.Stories));
            Assert.That(Has(menu, "Dialog"), Is.False, "the wheel is only back when the shop was opened from it");
        }

        [UnityTest]
        public IEnumerator TheDeckFollowsAHorizontalDragAndSettlesOnTheNextCard()
        {
            var menu = Open();
            yield return null;
            var deck = menu.Root.GetComponentsInChildren<Transform>().First(t => t.name == "Deck").gameObject;
            var cards = StoryCatalog.All.Select(s => menu.Root.GetComponentsInChildren<Transform>().First(t => t.name == "Card " + s.Id)).ToList();
            Assert.That(((RectTransform)cards[0]).anchoredPosition.x, Is.EqualTo(0).Within(.5f), "the first story is in the middle");

            var ped = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { delta = new Vector2(-4 * Viewport.Scale, 0) };
            UnityEngine.EventSystems.ExecuteEvents.Execute(deck, ped, UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
            ped.delta = new Vector2(-200 * Viewport.Scale, 0);
            UnityEngine.EventSystems.ExecuteEvents.Execute(deck, ped, UnityEngine.EventSystems.ExecuteEvents.dragHandler);
            UnityEngine.EventSystems.ExecuteEvents.Execute(deck, ped, UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
            yield return null;
            Assert.That(((RectTransform)cards[1]).anchoredPosition.x, Is.EqualTo(0).Within(.5f), "the next story came to the middle");
            Assert.That(((RectTransform)cards[0]).anchoredPosition.x, Is.LessThan(-100), "the first one went to the left");
        }

        [UnityTest]
        public IEnumerator AnotherSectionClosesAnOpenPopup()
        {
            var menu = Open();
            menu.ShowRewards();
            yield return null;
            Assert.That(Has(menu, "Dialog"), Is.True);
            menu.Select(MenuScreen.Tab.Chat);
            yield return null;
            Assert.That(Has(menu, "Dialog"), Is.False, "the rewards do not stay over the chats");
        }
    }
}
