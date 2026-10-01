using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LegacyThroughTime.Prototype
{
    /// The screens demo: every screen in a row (docs/brainstorm/templates/prototype-heritage.html).
    /// Tap to move on; a choice moves on too. Back (Android) or Left arrow goes one screen back.
    /// The StoryPlayer is the default; this one is added by hand, e.g. in the tests.
    public sealed class SessionPrototype : MonoBehaviour
    {
        readonly Bank bank = new();
        List<Page> flow;
        ScreenHost host;
        int index;
        int lastWidth, lastHeight;

        internal PageView Current => host?.Current;
        internal int Index => index;
        internal int Count => flow.Count;

        void Awake()
        {
            Stage.Setup();
            lastWidth = Screen.width; lastHeight = Screen.height;
            var canvas = Stage.BuildCanvas();
            Stage.BuildBackground(canvas);
            host = new ScreenHost(this, canvas);
            flow = DemoFlow.Pages();
            Go(0);
        }

        void Update()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width; lastHeight = Screen.height;
                Viewport.Measure(); Go(index);       // rotation, split screen
            }
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)) Go(index - 1);
        }

        void Go(int i)
        {
            index = Mathf.Clamp(i, 0, flow.Count - 1);
            host.Show(flow[index], bank, Next);
        }

        void Next()
        {
            if (index == flow.Count - 1) { bank.Set(Bank.Start); Go(0); }
            else Go(index + 1);
        }
    }
}
