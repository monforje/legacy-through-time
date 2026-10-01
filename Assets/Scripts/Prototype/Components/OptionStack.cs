using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// A column of option buttons: measures itself, stands under a plate or inside a panel, slides in one
    /// by one, and turns into "chosen + dimmed" when an answer is taken.
    sealed class OptionStack
    {
        const float EntranceRise = 16, EntranceTime = .36f, EntranceStep = .07f;

        public readonly IReadOnlyList<OptionButton> Items;
        /// Height of the whole column, gaps included.
        public readonly float Height;

        public OptionStack(Transform parent, Option[] options, float width)
        {
            var items = new List<OptionButton>(options.Length);
            foreach (var o in options) items.Add(new OptionButton(parent, o, width));
            Items = items;
            var heights = new List<float>(items.Count);
            foreach (var b in items) heights.Add(b.Height);
            Height = PlateLayout.StackHeight(heights, Metrics.OptionGap);
        }

        public void SetClick(Func<Action, Action> guard, Action<OptionButton> picked)
        {
            foreach (var b in Items) { var button = b; button.Click = guard(() => picked(button)); }
        }

        /// Under a plate, from `bottom` up, between the given margins; above the plate in the draw order.
        public void PlaceUnderPlate(float left, float right, float bottom)
        {
            var y = bottom;
            for (var i = Items.Count - 1; i >= 0; i--)
            {
                var b = Items[i];
                b.Root.SetAsLastSibling();
                Ui.BottomRow(b.Root, left, right, y, b.Height);
                y += b.Height + Metrics.OptionGap;
                b.Rest = b.Root.anchoredPosition;
            }
        }

        /// Inside a panel, from `top` (distance from the panel's top edge) down, centred, `width` wide.
        public void PlaceInPanel(RectTransform panel, float top, float width)
        {
            var y = -top;
            foreach (var b in Items)
            {
                b.Root.SetParent(panel, false);
                Ui.Box(b.Root, A.TopCenter, A.TopCenter, new Vector2(0, y), new Vector2(width, b.Height));
                b.Rest = b.Root.anchoredPosition;
                y -= b.Height + Metrics.OptionGap;
            }
        }

        /// Hidden below their place, ready for CascadeIn.
        public void HideForEntrance()
        {
            foreach (var b in Items) { b.Group.alpha = 0; b.Root.anchoredPosition = b.Rest + new Vector2(0, -EntranceRise); }
        }

        public void CascadeIn(IAnimator anim)
        {
            for (var k = 0; k < Items.Count; k++)
            {
                var b = Items[k];
                anim.Play(Tween.To(EntranceTime, e => { b.Group.alpha = e; b.Root.anchoredPosition = b.Rest + new Vector2(0, -EntranceRise * (1 - e)); }, Easing.OutExpo, k * EntranceStep));
            }
        }

        public void Decide(OptionButton chosen, IAnimator anim)
        {
            foreach (var b in Items)
                if (b == chosen) b.MarkChosen(anim); else b.Dim();
        }

        /// Time ran out: nothing was chosen, every option dims.
        public void DimAll() { foreach (var b in Items) b.Dim(); }
    }
}
