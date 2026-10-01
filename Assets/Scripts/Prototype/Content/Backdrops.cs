using System;

namespace LegacyThroughTime.Prototype
{
    /// Hue/saturation/value of a location's stand-in colour (the painted backgrounds do not exist yet).
    readonly struct Tone
    {
        public readonly float H, S, V;
        public Tone(float h, float s, float v) { H = h; S = s; V = v; }
    }

    static class Backdrops
    {
        public static readonly Tone Default = new(.4f, .5f, .42f);
        public static readonly Tone Black = new(0, .2f, .02f);

        /// A tint per background id (`# bg: id` in the story): greens to teal by the hash of the id, plus the mood of the name.
        public static Tone For(string id)
        {
            if (string.IsNullOrEmpty(id)) return Default;
            if (id == "black") return Black;
            var hash = 17;
            unchecked { foreach (var c in id) hash = hash * 31 + c; }
            var h = .33f + (Math.Abs(hash % 100)) / 100f * .14f;          // greens to teal
            if (id.Contains("night")) return new Tone(.5f, .5f, .24f);
            if (id.Contains("dusk") || id.Contains("evening")) return new Tone(.06f + Math.Abs(hash % 10) / 100f, .42f, .34f);
            if (id.Contains("dawn")) return new Tone(.1f, .35f, .46f);
            if (id.Contains("dream")) return new Tone(.72f, .3f, .3f);
            return new Tone(h, .5f, .42f);
        }
    }
}
