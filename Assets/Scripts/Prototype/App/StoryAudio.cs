using System.Collections.Generic;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Sound of the story (Resources/Audio/<music|ambience|sfx|ui>/<name>.ogg; list and sources in docs/audio).
    /// Music and ambience follow the background of the scene (`# bg:`) and cross-fade when it changes; a one-shot
    /// effect (`# snd: name` on a line) is cut at the next scene change so that nothing keeps sounding out of place.
    /// A missing clip is just silence.
    sealed class StoryAudio
    {
        const float MusicVolume = .5f, AmbienceVolume = .6f, SfxVolume = .9f, UiVolume = .6f, FadeSeconds = 1.2f;

        // ---- which loop plays under which backgrounds; a background not listed leaves the music alone and fades the ambience out
        static readonly Dictionary<string, string> Music = new(), Ambience = new();

        static StoryAudio()
        {
            M("main_theme", "title");
            M("steppe_calm", "steppe_dawn", "tent_entrance", "bronze_mirror", "kalfak_embroidery", "heroine_room_dusk", "heroine_room_evening");
            M("tension_hall", "yusuf_hall", "yusuf_stare", "yusuf_table_slam", "katyk_bowl", "yusuf_letter", "kazan_envoys", "yusuf_confronts", "locked_room", "khan_face_to_face");
            M("road", "camp_yard_evening", "wagon_departure", "cart_road", "dice_game", "toll_road", "road_evening", "wagon_repair_help", "village_lights", "knife_closeup");
            M("chase", "camp_backyard", "camp_search", "stable_catch");
            M("dream", "dream_tower", "dream_fire");
            M("kazan", "kazan_gate", "kazan_panorama", "palace_corridor", "servants_search", "chamber_door_guard");
            M("mystery", "village_house_night", "village_house_dawn", "palm_closeup", "miniature_closeup", "overheard_servants", "wax_seal", "heroine_room_night");
            M("flashback", "flashback_embroidery", "flashback_katyk");
            M("khan_quiet", "khan_chamber", "khan_by_window", "khan_silence");

            A("steppe_wind", "title", "steppe_dawn", "heroine_room_evening");
            A("camp_day", "tent_entrance", "camp_yard_evening", "camp_backyard", "kazan_envoys", "yusuf_confronts");
            A("tent_interior", "heroine_room_dusk", "bronze_mirror", "kalfak_embroidery", "yusuf_hall", "yusuf_stare", "yusuf_table_slam", "katyk_bowl", "yusuf_letter", "locked_room");
            A("road_evening", "road_evening", "wagon_repair_help", "village_lights", "toll_road");
            A("village_night", "village_house_night", "village_house_dawn", "palm_closeup");
            A("dream_drone", "dream_tower", "dream_fire");
            A("kazan_city", "kazan_panorama", "kazan_gate");
            A("palace_hall", "palace_corridor", "servants_search", "overheard_servants", "chamber_door_guard", "khan_chamber", "khan_face_to_face", "khan_silence", "khan_by_window");
            A("night_room", "heroine_room_night", "wax_seal", "miniature_closeup");
        }

        static void M(string clip, params string[] ids) { foreach (var id in ids) Music[id] = clip; }
        static void A(string clip, params string[] ids) { foreach (var id in ids) Ambience[id] = clip; }

        /// Two sources per loop: the new clip fades in on one while the old one fades out on the other.
        sealed class Loop
        {
            readonly AudioSource[] sources = new AudioSource[2];
            readonly string folder;
            readonly float volume;
            int active;
            string clip;

            public Loop(Transform parent, string folder, float volume)
            {
                this.folder = folder; this.volume = volume;
                for (var i = 0; i < 2; i++)
                {
                    var s = new GameObject(folder + i).AddComponent<AudioSource>();
                    s.transform.SetParent(parent, false);
                    s.loop = true; s.playOnAwake = false; s.volume = 0;
                    sources[i] = s;
                }
            }

            public void Play(string name)
            {
                if (name == clip) return;
                clip = name;
                active = 1 - active;
                var next = sources[active];
                var data = name == null ? null : Resources.Load<AudioClip>($"Audio/{folder}/{name}");
                next.Stop(); next.volume = 0; next.clip = data;
                if (data != null) next.Play();
            }

            public void Tick(float dt)
            {
                for (var i = 0; i < 2; i++)
                {
                    var s = sources[i];
                    var goal = i == active && s.clip != null ? volume : 0;
                    s.volume = Mathf.MoveTowards(s.volume, goal, volume * dt / FadeSeconds);
                    if (s.isPlaying && s.volume <= 0 && goal <= 0) s.Stop();
                }
            }
        }

        readonly Loop music, ambience;
        readonly AudioSource sfx, ui;
        string scene;

        public StoryAudio(Transform parent)
        {
            music = new Loop(parent, "music", MusicVolume);
            ambience = new Loop(parent, "ambience", AmbienceVolume);
            sfx = Source(parent, "sfx"); ui = Source(parent, "ui");
        }

        static AudioSource Source(Transform parent, string name)
        {
            var s = new GameObject(name).AddComponent<AudioSource>();
            s.transform.SetParent(parent, false); s.playOnAwake = false;
            return s;
        }

        /// A new location: the loops follow it, a running effect is cut. `black` and unlisted places fade the ambience out;
        /// `black` also fades the music out (a listed place changes it, an unlisted one leaves it).
        public void Scene(string backgroundId)
        {
            if (backgroundId == scene) return;
            scene = backgroundId;
            sfx.Stop();
            if (scene == "black") { music.Play(null); ambience.Play(null); return; }
            if (Music.TryGetValue(scene ?? "", out var m)) music.Play(m);
            ambience.Play(Ambience.TryGetValue(scene ?? "", out var a) ? a : null);
        }

        /// One-shot effect of the story; it stops at the next scene change.
        public void Sfx(string name)
        {
            var clip = Clip("Audio/sfx/" + name);
            if (clip == null) return;
            sfx.Stop(); sfx.clip = clip; sfx.volume = SfxVolume; sfx.Play();
        }

        public void Ui(string name, float volume = 1)
        {
            var clip = Clip("Audio/ui/" + name);
            if (clip != null) ui.PlayOneShot(clip, UiVolume * volume);
        }

        static readonly System.Collections.Generic.Dictionary<string, AudioClip> clips = new();

        /// Short effects are loaded once: the same click or plate sound plays dozens of times per episode.
        static AudioClip Clip(string path) =>
            clips.TryGetValue(path, out var c) && c ? c : clips[path] = Resources.Load<AudioClip>(path);

        public void Tick(float deltaTime) { music.Tick(deltaTime); ambience.Tick(deltaTime); }
    }
}
