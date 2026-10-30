using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// Who stands on the screen in one story: the names of the speakers -> the characters, the places where the hero
    /// is in travelling clothes, how tall each one is. The pictures are Resources/Art/Characters/<name>.png.
    sealed class CastProfile
    {
        public string HeroLabel;                           // the label of the hero's thoughts
        public string[] HeroSpeakers;                      // speakers on the hero's side (the plate on the left); a name may end with one of these
        public string[] HeroIds;                           // characters that stand on the hero's side of the screen
        public Dictionary<string, string> Ids;             // the speaker's name -> the character
        public HashSet<string> Travel = new();             // backgrounds where the hero wears the travelling outfit
        public Dictionary<string, float> Heights = new();  // next to the hero (1): a child is shorter, a man a bit taller
        public string OutfitHero;                          // the hero that has an outfit in the name of the picture (`suyumbike_court`); null: none
        public string KitPrefix;                           // the story's own UI sprites in Resources/Heritage3D (`sa-btn`); null: the main kit
    }

    /// Who stands on the screen and which picture of them: the speaker's name -> the character, the background -> the
    /// outfit of the heroine, the `# emo:` tag -> the face. The profile is that of the story being read (`Use`).
    static class Cast
    {
        public const string Heroine = "suyumbike", Child = "suyumbike_child";

        static readonly CastProfile goldenCage = new()
        {
            HeroLabel = "Сююмбике",
            HeroSpeakers = new[] { "Сююмбике" },             // also "Маленькая Сююмбике"
            HeroIds = new[] { Heroine, Child },
            OutfitHero = Heroine,
            Ids = new()
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
            },
            Travel = new()
            {
                "steppe_dawn", "camp_yard_evening", "camp_backyard", "camp_night", "cart_road", "road_evening", "toll_road",
                "village_lights", "village_house_night", "village_house_dawn", "kazan_gate", "kazan_panorama",
            },
            Heights = new()
            {
                ["suyumbike_child"] = .7f, ["boy_dream"] = .5f, ["woman_a"] = .96f, ["safiya"] = .98f, ["yusuf"] = 1.03f, ["chura_timer"] = 1.01f,
            },
        };

        static readonly CastProfile sikhoteAlin = new()
        {
            HeroLabel = "Арсеньев",
            KitPrefix = "sa-",
            HeroSpeakers = new[] { "Володя", "Арсеньев" },
            HeroIds = new[] { "volodya", "arsenyev" },
            Ids = new()
            {
                ["Володя"] = "volodya",
                ["Мать"] = "mother",
                ["Отец"] = "father",
                ["Арсеньев"] = "arsenyev",
                ["Матрос"] = "sailor",
                ["Грузчик"] = "porter",
                ["Пароходный офицер"] = "ship_officer",
                ["Городовой"] = "harbour_policeman",
            },
            Heights = new() { ["volodya"] = .72f, ["mother"] = .96f, ["father"] = 1.02f },
        };

        static readonly Dictionary<string, CastProfile> profiles = new()
        {
            ["golden-cage"] = goldenCage,
            ["sikhote-alin"] = sikhoteAlin,
        };

        static CastProfile profile = goldenCage;

        /// The cast of the story being read; a story without its own keeps the current one.
        public static void Use(string storyId)
        {
            if (storyId != null && profiles.TryGetValue(storyId, out var p)) profile = p;
        }

        /// The prefix of the story's own UI sprites (Art.StoryKit), null for the main kit.
        public static string KitPrefix => profile.KitPrefix;

        /// The label of the hero's thoughts and the speaker that stands for the hero in them.
        public static string HeroLabel => profile.HeroLabel;

        /// Is this speaker on the hero's side (the plate on the left)?
        public static bool IsHeroSpeaker(string speaker)
        {
            if (speaker == null) return false;
            foreach (var name in profile.HeroSpeakers)
                if (speaker.EndsWith(name, StringComparison.Ordinal)) return true;
            return false;
        }

        /// Does this character stand on the hero's side of the screen?
        public static bool IsHero(string id) => id != null && Array.IndexOf(profile.HeroIds, id) >= 0;

        /// How tall the character is next to the hero (1).
        public static float Height(string id) => id != null && profile.Heights.TryGetValue(id, out var h) ? h : 1f;

        /// The character of a speaker; null for a voice without a picture (a servant, "Голос").
        public static string Id(string speaker) => speaker != null && profile.Ids.TryGetValue(speaker, out var id) ? id : null;

        public static bool Travel(string background) => background != null && profile.Travel.Contains(background);

        /// Name of the picture: `safiya`, `safiya_sad`; the heroine's has her outfit: `suyumbike_court_sad`.
        public static string Sprite(string id, string emotion, bool travelOutfit)
        {
            if (id != null && id == profile.OutfitHero) id = travelOutfit ? id + "_travel" : id + "_court";
            return string.IsNullOrEmpty(emotion) || emotion == "neutral" ? id : id + "_" + emotion;
        }

        /// The picture without a face, when the one asked for was not drawn: `suyumbike_court_laugh` -> `suyumbike_court`.
        public static string Plain(string id, bool travelOutfit) => Sprite(id, null, travelOutfit);
    }
}
