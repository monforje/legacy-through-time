using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Tap target with press feedback: swaps the sprite and scales while the finger is down.
    sealed class PressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Action Click;
        public bool Interactable = true;
        public Image Face;
        public Sprite Normal, Pressed;
        public float PressScale = .97f;
        bool down;

        public void OnPointerDown(PointerEventData e) { if (Interactable) Set(true); }
        public void OnPointerUp(PointerEventData e) => Set(false);
        public void OnPointerExit(PointerEventData e) => Set(false);
        public void OnPointerClick(PointerEventData e) { if (Interactable) Click?.Invoke(); }

        void Set(bool on)
        {
            if (down == on) return;
            down = on;
            transform.localScale = on ? Vector3.one * PressScale : Vector3.one;
            if (Face && Pressed && Interactable) Face.sprite = on ? Pressed : Normal;
        }

        /// Drops the pressed look (the button is about to change state).
        public void Settle() { down = false; transform.localScale = Vector3.one; }

        /// Makes any image react like a button.
        public static PressButton On(GameObject go, Action click, float pressScale = .97f, Image face = null, string pressedSprite = null)
        {
            var b = go.AddComponent<PressButton>();
            b.Click = click; b.PressScale = pressScale;
            if (face)
            {
                face.raycastTarget = true;
                b.Face = face; b.Normal = face.sprite;
                if (pressedSprite != null) b.Pressed = Art.Sprite(pressedSprite);
            }
            return b;
        }
    }

    /// A plain tap on a full-screen catcher.
    sealed class Tap : MonoBehaviour, IPointerClickHandler
    {
        public Action Click;
        public void OnPointerClick(PointerEventData e) => Click?.Invoke();
    }
}
