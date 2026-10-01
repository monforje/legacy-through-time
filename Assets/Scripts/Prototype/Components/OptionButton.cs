using System;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// One answer: a green button with the solar rhombus, the text, an optional price and a chevron.
    /// States: normal, pressed (while the finger is down), chosen, dimmed (an option that was not taken).
    sealed class OptionButton
    {
        const float MinHeight = 50, PadLeft = 40, PadRight = 34, CostWidth = 52;

        public readonly Option Data;
        public readonly RectTransform Root;
        public readonly CanvasGroup Group;
        public float Height { get; set; }
        /// Where the button stands when it is on screen (the entrance animation starts below it).
        public Vector2 Rest;

        readonly PressButton button;
        readonly Image face, chevron, coin;
        readonly Text label, cost;

        /// `reserveCost`: keep room and objects for a price that changes later (the confirm button of a picker).
        public OptionButton(Transform parent, Option option, float width, bool reserveCost = false)
        {
            Data = option;
            var hasCost = option.Cost > 0 || reserveCost;
            var costWidth = hasCost ? CostWidth : 0f;

            Root = Ui.Rect(parent, "Option");
            face = Root.gameObject.AddComponent<Image>();
            face.sprite = Art.Sprite("btn"); face.type = Image.Type.Sliced;
            Ui.Lift(face, 5, .34f);
            Group = Root.gameObject.AddComponent<CanvasGroup>();

            label = Ui.Txt(Root, "Text", TextRules.NoBreaks(option.Text), Art.Action, Metrics.FsAction, Theme.Parchment);
            Ui.Emboss(label);
            Height = Mathf.Max(MinHeight, Ui.TextHeight(label, width - PadLeft - PadRight - costWidth) + 19);
            Ui.Stretch(label.rectTransform, PadLeft, 11, PadRight + costWidth, 8);

            Ui.Icon(Root, "Orn", "orn", A.MidLeft, A.MidLeft, new Vector2(12, 1), 22);
            chevron = Ui.Icon(Root, "Chevron", "chevron", A.MidRight, A.MidRight, new Vector2(-14, 0), 12, Theme.Parchment);
            if (hasCost)
            {
                coin = Ui.Icon(Root, "Coin", "coin", A.MidRight, A.MidRight, new Vector2(-34, 0), 21);
                cost = Ui.Txt(Root, "Cost", "", Art.Action, Metrics.FsAction, Theme.Parchment, TextAnchor.MiddleRight);
                Ui.Emboss(cost);
                Ui.Box(cost.rectTransform, A.MidRight, A.MidRight, new Vector2(-60, 0), new Vector2(30, 30));
                SetCost(option.Cost);
            }

            button = PressButton.On(Root.gameObject, null, .97f, face, "btn-pressed");
        }

        public Action Click { set => button.Click = value; }
        public bool Interactable { get => button.Interactable; set => button.Interactable = value; }

        /// Shows or hides the price (needs `reserveCost` or an option that had a price).
        public void SetCost(int amount)
        {
            if (!cost) return;
            cost.text = amount.ToString();
            cost.gameObject.SetActive(amount > 0); coin.gameObject.SetActive(amount > 0);
        }

        public void MarkChosen(IAnimator anim)
        {
            button.Settle();
            button.Interactable = false;
            face.sprite = Art.Sprite("btn-chosen");
            label.color = Theme.Ink; chevron.color = Theme.Scarlet;
            if (cost) cost.color = Theme.Ink;
            anim.Play(Effects.Pop(Root));
        }

        public void Dim()
        {
            button.Interactable = false;
            Group.alpha = .4f;
            Root.localScale = Vector3.one * .97f;
        }
    }
}
