using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Choice with arrows: a value in a field between two arrow keys, a "Выбрать." button below, and (optionally)
    /// the coin balance above. Paid values cost coins from the Bank; a purse that is too thin shakes the button.
    sealed class PickerPanel : IDisposable
    {
        const float Top = 38, FieldHeight = 64, ArrowWidth = 44, ArrowHeight = 50, Side = 16, ButtonGap = 14;

        public readonly SlidePanel Panel;
        /// Raised with the confirmed value once the payment (if any) went through.
        public event Action<Option> Confirmed;

        readonly PickerSpec spec;
        readonly Bank bank;
        readonly IAnimator anim;
        readonly Text value;
        readonly OptionButton confirm;
        readonly BalancePill balance;
        int index;
        bool locked;

        public PickerPanel(Transform parent, PickerSpec spec, Bank bank, Func<Action, Action> guard, IAnimator anim)
        {
            this.spec = spec; this.bank = bank; this.anim = anim;
            Panel = new SlidePanel(parent, Metrics.PickerHeight, 34, spec.Label, .78f, anim);
            Panel.CollapseButton.Click = guard(Panel.Toggle);
            var inner = Metrics.PanelTextWidth;

            var field = Ui.Sliced(Panel.Root, "Field", "field");
            Ui.Box(field.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -Top), new Vector2(inner - 2 * 52, FieldHeight));
            value = Ui.Txt(field.rectTransform, "Value", "", Art.Sub, Metrics.FsValue, Theme.Ink, TextAnchor.MiddleCenter);
            Ui.Stretch(value.rectTransform, 10, 4, 10, 4);

            var arrowY = -(Top + (FieldHeight - ArrowHeight) / 2 + ArrowHeight / 2);
            Arrow(false, new Vector2(Side + ArrowWidth / 2, arrowY), guard);
            Arrow(true, new Vector2(Metrics.PlateWidth - Side - ArrowWidth / 2, arrowY), guard);

            confirm = new OptionButton(Panel.Root, new Option("Выбрать."), inner, reserveCost: true) { Height = 50 };
            Ui.Box(confirm.Root, A.TopCenter, A.TopCenter, new Vector2(0, -(Top + FieldHeight + ButtonGap)), new Vector2(inner, 50));
            confirm.Click = guard(Confirm);

            if (spec.ShowBalance)
            {
                balance = BalancePill.Interactive(Panel.Root, bank, guard, anim);
                Panel.Dissolve(balance.Group);
            }
            Show(0);
        }

        void Arrow(bool forward, Vector2 center, Func<Action, Action> guard)
        {
            var key = Ui.Sliced(Panel.Root, forward ? "Next" : "Prev", "key");
            Ui.Box(key.rectTransform, A.TopLeft, A.Center, center, new Vector2(ArrowWidth, ArrowHeight));
            Ui.Lift(key, 4, .34f);
            var chevron = Ui.Icon(key.transform, "Chevron", "chevron", A.Center, A.Center, new Vector2(0, 2), 14, Theme.Parchment);
            if (!forward) chevron.rectTransform.localScale = new Vector3(-1, 1, 1);
            PressButton.On(key.gameObject, guard(() => Step(forward ? 1 : -1)), .9f, key, "key-pressed");
        }

        void Step(int direction)
        {
            if (locked) return;
            index = (index + direction + spec.Values.Length) % spec.Values.Length;
            Show(direction);
        }

        void Show(int direction)
        {
            var v = spec.Values[index];
            value.text = TextRules.NoBreaks(v.Text);
            confirm.SetCost(v.Cost);
            if (direction == 0) return;
            anim.Play(Tween.To(.26f, e =>
            {
                value.rectTransform.anchoredPosition = new Vector2(direction * 22 * (1 - e), 0);
                value.color = Theme.Ink.WithAlpha(e);
            }));
        }

        void Confirm()
        {
            if (locked) return;
            var v = spec.Values[index];
            if (v.Cost > 0 && !bank.TrySpend(v.Cost)) { anim.Play(Effects.Shake(confirm.Root)); return; }
            locked = true;
            confirm.MarkChosen(anim);
            Confirmed?.Invoke(v);
        }

        public void Dispose() => balance?.Dispose();
    }
}
