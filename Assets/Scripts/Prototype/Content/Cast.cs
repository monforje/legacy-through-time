using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// Who stands on the screen and which picture of them: the speaker's name -> the character, the background -> the
    /// outfit of the heroine, the `# emo:` tag -> the face. The pictures are Resources/Art/Characters/<name>.png.
    static class Cast
    {
        public const string Heroine = "suyumbike", Child = "suyumbike_child";

        static readonly Dictionary<string, string> ids = new()
        {
            ["Сююмбике"] = Heroine,
            ["Маленькая Сююмбике"] = Child,
            ["Сафия"] = "safiya",
            ["Юсуф"] = "yusuf",
            ["Джан-Али"] = "jan_ali",
            ["Чура Тимер"] = "chura_timer",
            ["Чура Булат"] = "chura_bulat",
            ["Чура Идрис"] = "chura_idris",
            ["Мальчик"] = "boy_dream",
            ["Женский голос"] = "woman_a",
            ["Голос женщины"] = "woman_a",
        };

        /// Places where the heroine is in her travelling clothes; elsewhere in the festive caftan.
        static readonly HashSet<string> travel = new()
        {
            "steppe_dawn", "camp_yard_evening", "camp_backyard", "camp_night", "cart_road", "road_evening", "toll_road",
            "village_lights", "village_house_night", "village_house_dawn", "kazan_gate", "kazan_panorama",
        };

        /// The character of a speaker; null for a voice without a picture (a servant, "Голос").
        public static string Id(string speaker) => speaker != null && ids.TryGetValue(speaker, out var id) ? id : null;

        public static bool Travel(string background) => background != null && travel.Contains(background);

        /// Name of the picture: `safiya`, `safiya_sad`; the heroine's has her outfit: `suyumbike_court_sad`.
        public static string Sprite(string id, string emotion, bool travelOutfit)
        {
            if (id == Heroine) id = travelOutfit ? "suyumbike_travel" : "suyumbike_court";
            return string.IsNullOrEmpty(emotion) || emotion == "neutral" ? id : id + "_" + emotion;
        }

        /// The picture without a face, when the one asked for was not drawn: `suyumbike_court_laugh` -> `suyumbike_court`.
        public static string Plain(string id, bool travelOutfit) => Sprite(id, null, travelOutfit);
    }
}
