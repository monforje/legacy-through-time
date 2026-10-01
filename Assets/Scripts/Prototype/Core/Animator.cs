using System.Collections;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Where the coroutines of a view run. Components take it instead of a MonoBehaviour, so that a view built
    /// without animation (tests, the smoke run in the Editor) simply plays nothing.
    interface IAnimator
    {
        bool Enabled { get; }
        void Play(IEnumerator routine);
        void Stop();
    }

    sealed class NoAnimator : IAnimator
    {
        public static readonly NoAnimator Instance = new();
        public bool Enabled => false;
        public void Play(IEnumerator routine) { }
        public void Stop() { }
    }

    /// Runs coroutines on a component that lives on the view's root, so they die with the view.
    sealed class CoroutineAnimator : MonoBehaviour, IAnimator
    {
        public bool Enabled => true;
        public void Play(IEnumerator routine) => StartCoroutine(routine);
        public void Stop() => StopAllCoroutines();
    }
}
