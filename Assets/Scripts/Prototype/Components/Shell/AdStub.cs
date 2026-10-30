using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Where an ad clip will play once a provider is connected: a dialog with a plate and a button that wakes after a few
    /// seconds. A clip for a reward pays out only through the button; the cross closes it without a reward.
    sealed class AdStub
    {
        readonly ShellDialog dialog;
        readonly ShellButton button;
        readonly Action reward;
        readonly string label;
        int left = AdRules.ClipSeconds;

        public static AdStub Show(Transform parent, IAnimator anim, string title, bool rewarded, Action done) =>
            new AdStub(parent, anim, title, rewarded, done);

        AdStub(Transform parent, IAnimator anim, string title, bool rewarded, Action done)
        {
            reward = rewarded ? done : null;
            label = rewarded ? "Получить награду" : "Закрыть";
            dialog = new ShellDialog(parent, title, anim, rewarded ? null : done);
            var plate = Kit.Round(dialog.Content, "Clip", Kit.Surface2, Kit.RadiusM);
            Ui.TopRow(plate.rectTransform, 0, 0, 0, 280);
            var text = Ui.Txt(plate.rectTransform, "Text", "Здесь будет рекламный ролик.\nРекламный провайдер пока не подключён.",
                              Kit.F700, Kit.FsBody, Kit.Muted, TextAnchor.MiddleCenter);
            Ui.Stretch(text.rectTransform, 16, 16, 16, 16);
            button = Parts.Button(dialog.Content, label, ButtonStyle.Primary, OnTap, Kit.Touch);
            Ui.BottomRow(button.Root, 0, 0, 0, Kit.Touch);
            if (anim == null || !anim.Enabled) left = 0;
            Refresh();
            if (left > 0) anim.Play(Count());
        }

        void Refresh()
        {
            button.SetEnabled(left <= 0);
            button.SetLabel(left > 0 ? $"{label} ({left})" : label);
        }

        IEnumerator Count()
        {
            while (left > 0 && dialog.IsOpen)
            {
                yield return new WaitForSecondsRealtime(1f);
                if (!dialog.IsOpen) yield break;
                left--;
                Refresh();
            }
        }

        void OnTap()
        {
            if (left > 0 || !dialog.IsOpen) return;
            dialog.Close();
            reward?.Invoke();
        }
    }
}
