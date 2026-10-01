using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The colour of the stand-in background eases to the tint of the current location.
    sealed class BackdropTint
    {
        const float Speed = 6;
        readonly Image image;
        Color goal = Theme.Backdrop;

        public BackdropTint(Image image) { this.image = image; }

        public void Set(string backgroundId) => goal = string.IsNullOrEmpty(backgroundId) ? Theme.Backdrop : Theme.FromTone(Backdrops.For(backgroundId));
        public void Tick(float deltaTime) => image.color = Color.Lerp(image.color, goal, 1 - Mathf.Exp(-Speed * deltaTime));
    }
}
