using System.Collections;
using System.Linq;
using LegacyThroughTime.Prototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace LegacyThroughTime.PrototypeTests
{
    /// Plays the whole of episode 1 with synthetic taps, always taking the first choice.
    /// Every beat becomes a real screen with its animations; any logged error fails the test.
    public sealed class StoryWalkThrough
    {
        static void Click(GameObject go) => ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        [UnityTest, Timeout(600000)]
        public IEnumerator TheEpisodeCanBePlayedToTheEnd()
        {
            var player = new GameObject("StoryPlayer").AddComponent<StoryPlayer>();
            player.AutoStart = "golden-cage"; player.Persist = false;      // straight into the story, the reader's save untouched
            var stop = Time.realtimeSinceStartup + 400;
            var screens = 0;
            PageView last = null;
            while (!player.Ended && Time.realtimeSinceStartup < stop)
            {
                yield return new WaitForSecondsRealtime(.05f);
                var view = player.Current;
                if (view == null) continue;
                if (view != last) { screens++; last = view; }

                // an option if there are any (a tap while the text is typed only finishes the typing), else a tap on the screen
                var option = view.Root.GetComponentsInChildren<PressButton>().FirstOrDefault(b => b.name == "Option" && b.Interactable);
                if (option) Click(option.gameObject);
                else Click(view.Root.Find("Tap").gameObject);
            }

            Assert.That(player.Ended, "the story did not reach its end");
            Assert.That(screens, Is.GreaterThan(200), "screens shown");
            Assert.That(player.Machine.GetInt("steppe_memory"), Is.GreaterThanOrEqualTo(0));
            Object.Destroy(player.gameObject);
        }
    }
}
