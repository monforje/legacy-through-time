using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Options in their own panel at the bottom of the screen, introduced by a name tag ("Сафия ждёт ответа…").
    sealed class ChoicePanel
    {
        public readonly SlidePanel Panel;
        public readonly OptionStack Stack;

        public ChoicePanel(Transform parent, ChoiceSpec spec, IAnimator anim)
        {
            var inner = Metrics.PanelTextWidth;
            // the options are measured first (the panel is as tall as they are), then moved into the panel
            var scratch = Ui.Rect(parent, "scratch");
            Stack = new OptionStack(scratch, spec.Items, inner);
            var height = Metrics.PanelPaddingTop + Stack.Height + Metrics.PanelPaddingBottom;
            Panel = new SlidePanel(parent, height, 30, spec.PanelLabel, .7f, anim);
            Stack.PlaceInPanel(Panel.Root, Metrics.PanelPaddingTop, inner);
            Object.Destroy(scratch.gameObject);
        }
    }
}
