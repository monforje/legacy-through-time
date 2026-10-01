using System;
using System.Globalization;

namespace LegacyThroughTime.Prototype
{
    /// The version of a build: the moment it was made, "yyyy-MM-dd-HHmm" (local time). It is Application.version of the
    /// APK and the tag of its GitHub release (`task publish`), so the newest release is simply the greatest string.
    static class BuildStamp
    {
        public const string Format = "yyyy-MM-dd-HHmm";
        static readonly DateTime Epoch = new DateTime(2026, 1, 1);

        public static string Of(DateTime time) => time.ToString(Format, CultureInfo.InvariantCulture);

        public static bool IsStamp(string s) =>
            s != null && DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

        /// Android versionCode: minutes since 2026, growing with every build (an install over an older build is allowed).
        public static int VersionCode(DateTime time) => Math.Max(1, (int)(time - Epoch).TotalMinutes);

        /// A release is worth offering when it is a stamp newer than this build; a build without a stamp (made before
        /// stamps existed, "1.0") is older than any release.
        public static bool IsNewer(string release, string current) =>
            IsStamp(release) && (!IsStamp(current) || string.CompareOrdinal(release, current) > 0);
    }
}
