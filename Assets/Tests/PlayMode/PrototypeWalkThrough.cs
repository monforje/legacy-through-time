using System.Collections;
using System.Linq;
using LegacyThroughTime.Prototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LegacyThroughTime.PrototypeTests
{
    /// Plays the whole prototype with synthetic taps: every screen builds, text typing and
    /// transitions run, a choice of any kind leads on, and the last screen restarts the flow.
    /// Any logged error fails the test (Unity's default for UnityTest).
    public sealed class PrototypeWalkThrough
    {
        static void Click(GameObject go) => ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        [UnityTest]
        public IEnumerator EveryScreenCanBePassed()
        {
            var proto = new GameObject("Prototype").AddComponent<SessionPrototype>();
            yield return null;
            Assert.That(proto.Count, Is.EqualTo(DemoFlow.Order.Length + 1));

            for (var step = 0; step < proto.Count; step++)
            {
                Assert.That(proto.Index, Is.EqualTo(step), "flow index");
                var view = proto.Current;
                yield return new WaitForSeconds(.35f);                       // entrance animation

                // One action at a time until the screen changes. While the plate text is being typed out an action
                // only finishes the typing (a tap shows the rest, buttons guard the same way), so a few tries are normal.
                for (var tries = 0; tries < 6 && proto.Index == step; tries++)
                {
                    var buttons = view.Root.GetComponentsInChildren<PressButton>().Where(b => b.Interactable).ToList();
                    var option = buttons.FirstOrDefault(b => b.name == "Option");
                    var confirm = buttons.FirstOrDefault(b => b.GetComponentInChildren<Text>()?.text == "Выбрать.");
                    if (option) Click(option.gameObject);
                    else if (confirm) Click(confirm.gameObject);
                    else Click(view.Root.Find("Tap").gameObject);
                    yield return new WaitForSeconds(.3f);
                }
                Assert.That(proto.Index, Is.EqualTo(step + 1) | Is.EqualTo(0), "one screen at a time");
            }

            Assert.That(proto.Index, Is.EqualTo(0), "the last screen restarts the flow");
            Object.Destroy(proto.gameObject);
        }
    }
}
