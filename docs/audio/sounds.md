# Звуки эпизода 1 «Золотая клетка»

Список того, что нужно записать или сгенерировать. В игре звука пока нет: сейчас реплики `# sfx` показываются только подписью «Звук». Подключим, когда файлы будут готовы.

Куда класть: `Assets/Resources/Audio/<папка>/<имя>.ogg` (ogg, 44.1 kHz, стерео для музыки и амбиента, моно для коротких эффектов). Имя файла = id.

Промты на английском: так лучше понимают генераторы звука (ElevenLabs SFX, Stable Audio и т.п.) и музыки (Suno, Udio). Заметки на русском.

## Общий промт: звуковой стиль (вставлен в каждый промт музыки и амбиента)

```
SOUND STYLE: historical Tatar-Nogai steppe and Kazan Khanate, 16th century. Acoustic, organic, intimate. Instruments: kobyz (bowed horsehair fiddle), kurai (reed flute), dumbra (long-necked lute), frame drum (daf), jaw harp (kubyz), soft low strings, throat-singing drone, a female folk vocalise with no words. Cinematic, emotional, restrained, slow. Warm, dark and earthy. No electronic synths, no modern drums, no choir pop, no vocals with lyrics.
```

---

## 1. Музыка (петли и стингеры)

| Файл | Длина | Петля | Где |
|---|---|---|---|
| `music/main_theme.ogg` | 1–2 мин | да | заставка `title`, экран загрузки |
| `music/steppe_calm.ogg` | 2 мин | да | сцена 1 (`steppe_dawn`, `tent_entrance`, `bronze_mirror`, `kalfak_embroidery`), финал (`heroine_room_evening`) |
| `music/tension_hall.ogg` | 2 мин | да | сцена 2 (`yusuf_hall`, `yusuf_stare`, `yusuf_letter`, `kazan_envoys`), `yusuf_confronts`, `khan_face_to_face` |
| `music/road.ogg` | 2 мин | да | `cart_road`, `dice_game`, `toll_road`, `wagon_departure`, `road_evening` |
| `music/chase.ogg` | 1 мин | да | `camp_search`, `stable_catch` |
| `music/dream.ogg` | 1,5 мин | да | `dream_tower`, `dream_fire` |
| `music/kazan.ogg` | 2 мин | да | `kazan_gate`, `kazan_panorama`, `palace_corridor`, `servants_search` |
| `music/mystery.ogg` | 2 мин | да | `miniature_closeup`, `overheard_servants`, `wax_seal`, `heroine_room_night` |
| `music/flashback.ogg` | 1 мин | да | `flashback_embroidery`, `flashback_katyk` |
| `music/khan_quiet.ogg` | 2 мин | да | `khan_chamber`, `khan_by_window`, `khan_silence` |
| `music/episode_end.ogg` | 20 сек | нет | чёрный экран в конце, перед «ЭПИЗОД 1» |

Промты (после каждого добавь общий промт «звуковой стиль»):

**main_theme**
```
Main theme. Slow, epic and fateful. A lone female vocalise rises over a kobyz drone, then the dumbra enters with a steady pulse and soft frame drum, building to a warm, hopeful finale with strings. Feels like a sunrise over the steppe and a heavy journey ahead. Loopable. Around 70 BPM.
```
**steppe_calm**
```
Quiet morning on the steppe. Very soft kurai flute melody, light kobyz long notes, distant wind, almost no percussion. Melancholic, nostalgic, tender. Loopable, 60 BPM.
```
**tension_hall**
```
Tense family confrontation. Low kobyz drone, sparse low frame drum hits, a dumbra plucking a nervous slow pattern, silence between phrases. Cold and restrained, no melody resolution. Loopable, 55 BPM.
```
**road**
```
Travelling music on a country road. Gentle steady dumbra rhythm like wagon wheels, light frame drum, a curious kurai flute theme, warm and a bit playful with a hidden unease. Loopable, 85 BPM.
```
**chase**
```
Short tense chase in a camp at night. Fast frame drum and hand drum, urgent kobyz ostinato, dumbra strumming, rising pressure, no melody. Loopable, 130 BPM.
```
**dream**
```
Prophetic dream. Slow reversed and reverberant kobyz, deep throat-singing drone, distant bells, a child's faint voice-like hum, a growing low rumble. Eerie, weightless, unstable. Loopable, no rhythm.
```
**kazan**
```
Arrival in the khan's city. Richer arrangement: dumbra, kurai, frame drum, soft strings with an oriental feel, bustle and ceremony. Mysterious and slightly threatening beneath the grandeur. Loopable, 75 BPM.
```
**mystery**
```
Palace intrigue and secrets. Sparse plucked dumbra, soft pizzicato strings, muted frame drum like a heartbeat, a low drone with occasional high harmonics. Quiet, suspicious. Loopable, 60 BPM.
```
**flashback**
```
Warm memory of childhood. A soft vocalise of a mother, light kurai, gentle plucked strings, faded and slightly lo-fi like an old memory, a feeling of safety and loss. Loopable, 65 BPM.
```
**khan_quiet**
```
A tense private conversation between a young khan and a bride. Soft strings and slow dumbra, a single warm kurai line, long silences, restrained emotion with a hint of romance. Loopable, 58 BPM.
```
**episode_end**
```
Episode ending stinger, 20 seconds. A single kobyz note growing, one deep drum, a gentle vocalise resolving and fading into silence. Determined and open-ended.
```

---

## 2. Амбиент (петли на фоне, тихо, под музыку)

Общий промт для всех: `... seamless loop, no music, no voices, natural recording, stereo.`

| Файл | Где (`# bg:`) | Промт |
|---|---|---|
| `ambience/steppe_wind.ogg` | `steppe_dawn`, `title`, `heroine_room_evening` (тише) | Wind through dry steppe grass, distant birds at dawn, a faraway horse neigh, faint tent cloth flapping. 60 s |
| `ambience/camp_day.ogg` | `tent_entrance`, `camp_yard_evening`, `camp_backyard`, `kazan_envoys`, `yusuf_confronts` | Nomad camp, soft murmur of people, horses stamping and snorting, clinking harness, a cauldron simmering, wind on felt. 60 s |
| `ambience/tent_interior.ogg` | `heroine_room_dusk`, `bronze_mirror`, `kalfak_embroidery`, `yusuf_hall`, `yusuf_stare`, `yusuf_letter`, `katyk_bowl`, `locked_room` | Inside a felt tent, muffled wind outside, soft fabric rustle, quiet fire crackle, very low room tone. 60 s |
| `ambience/camp_night_search.ogg` | `camp_search`, `stable_catch` | Night camp in a hurry: distant shouts, whistles, running footsteps, horses neighing, torches crackling, dogs barking. 45 s |
| `ambience/wagon_inside.ogg` | `cart_road`, `dice_game`, `wagon_departure` | Inside a wooden covered wagon on a dirt road: rhythmic wheel creak, wood strain, horse hooves muffled, harness jingle, light wind. 60 s |
| `ambience/road_evening.ogg` | `road_evening`, `wagon_repair_help`, `village_lights`, `toll_road` | Evening on a country road: crickets, distant dogs, a horse snorting, wind, men's low voices and tools, in `toll_road` add faint merchants' calls. 60 s |
| `ambience/village_night.ogg` | `village_house_night`, `village_house_dawn` | Wooden house at night: soft hearth crackle, wind in the roof, a distant dog, owl, wooden beams creak; for the dawn version add the first birds. 60 s |
| `ambience/dream_drone.ogg` | `dream_tower`, `dream_fire` | Dream soundscape: wind on a high tower, deep rumble, reversed whispers, distant fire roar growing, no clear sounds. 60 s |
| `ambience/kazan_city.ogg` | `kazan_panorama`, `kazan_gate` | Medieval city: crowd murmur, merchants calling, hammering of smiths, carts on stones, boats on a river, distant call to prayer-like voice. 60 s |
| `ambience/palace_hall.ogg` | `palace_corridor`, `servants_search`, `overheard_servants`, `chamber_door_guard`, `khan_chamber`, `khan_face_to_face`, `khan_silence`, `khan_by_window` | Quiet palace interior: reverberant room tone, distant footsteps on carpet, faint murmur of servants, a far door closing. 60 s |
| `ambience/night_room.ogg` | `heroine_room_night`, `wax_seal`, `miniature_closeup` | A quiet room at night: soft oil lamp flicker, light wind at a lattice window, a faint far-away watchman's step. 60 s |

---

## 3. Звуки из текста истории (теги `# sfx` и ремарки)

Короткие, моно, один раз на реплике. Первые девять — это буквально строки `# sfx` из `episode-01.ink`.

| Файл | Строка / сцена | Промт | Длина |
|---|---|---|---|
| `sfx/kobyz_motif.ogg` | сцена 1: «Тихий вокальный мотив, затем звук кобыза» | A soft female vocalise, then a kobyz plays a short melancholic phrase, intimate and slightly distant | 8 с |
| `sfx/hooves_arrive.ogg` | сцена 2: «Копыта» | A horse galloping into a camp yard and stopping, hooves on packed earth, dust and a snort | 4 с |
| `sfx/spoon_on_bowl.ogg` | флешбэк: «Деревянная ложка касается чаши» | A wooden spoon touches a wooden bowl once, soft and warm | 1 с |
| `sfx/ceramic_on_wood.ogg` | «Керамика ударяется о дерево» | A hand slams on a wooden table, a bowl rattles and a ceramic cup knocks hard on wood | 2 с |
| `sfx/wagon_door_close.ogg` | сцена 3: «Дверца закрывается» | A wooden wagon door closes with a heavy thud and latch click, muffled by cloth | 1,5 с |
| `sfx/camp_alarm.ogg` | ветка побега: «Свист, крики, лошади» | Men whistling and shouting in an alarm, horses neighing and running, a camp erupts, night | 5 с |
| `sfx/wagon_crack.ogg` | сцена 4: «Хруст. Повозка резко останавливается» | A loud wood crack, a wheel breaking, the wagon jerks and stops, harness jingles, a horse stops short | 3 с |
| `sfx/music_mute.ogg` | сон: «Музыка становится глуше» | Not a sound, but an effect: low-pass filter fade on the music over 2 s (делаем в коде, файла не нужно) | — |
| `sfx/wind_gust.ogg` | финал: «Ветер» | A long soft wind gust through a lattice window, a curtain moving | 5 с |

Ремарки, где звук тоже нужен (в тексте их нет как `# sfx`, добавим теги):

| Файл | Сцена | Промт | Длина |
|---|---|---|---|
| `sfx/horse_neigh.ogg` | сцена 1 («ржёт лошадь, звенит сбруя») | A single distant horse neigh with harness jingle in open air | 3 с |
| `sfx/cloth_unfold.ogg` | `tent_entrance` (Сафия разворачивает ткань) | Fabric and embroidered cloth being unfolded, soft rustle | 2 с |
| `sfx/wind_tent.ogg` | `tent_entrance` («Ветер бьёт в полотнище шатра») | Wind snapping a felt tent flap | 3 с |
| `sfx/horse_whinny_nervous.ogg` | `camp_yard_evening` | Nervous horses stamping and snorting, a few anxious whinnies | 4 с |
| `sfx/dice_roll.ogg` | `dice_game` | Three bone dice thrown and rolling on cloth over wood, coming to rest | 2 с |
| `sfx/dice_laugh.ogg` | `dice_game` | Two young women laughing together, warm and natural | 3 с |
| `sfx/knife_draw.ogg` | `knife_closeup` | A small knife slides from a leather sheath, then is hidden in cloth | 1,5 с |
| `sfx/paper_seal_break.ogg` | `yusuf_letter` | A wax seal cracked and a paper letter unfolded | 1,5 с |
| `sfx/stumble_rug.ogg` | `palace_corridor` | A foot catching a carpet, a gasp, a hand grabbing a sleeve | 1,5 с |
| `sfx/servant_crash.ogg` | `palace_corridor` | A servant bursts out of a door, a wooden door bang and chest knock | 1,5 с |
| `sfx/chest_rummage.ogg` | `servants_search` | Hasty rummaging through wooden chests, fabric thrown, copper clatter | 4 с |
| `sfx/wax_touch.ogg` | `wax_seal` | A fingertip slides over hard wax, soft scratch | 1 с |
| `sfx/footsteps_guard.ogg` | финал (шаги за дверью) | Slow measured footsteps of a guard behind a wooden door, pacing away and back | 6 с |
| `sfx/dream_gasp.ogg` | сон: «Нет!» → пробуждение | A sudden sharp gasp of breath, a heartbeat thump, then silence | 2 с |
| `sfx/fire_boom.ogg` | `dream_fire` | A deep rumble of an explosion, stone cracking, a roar of fire, reverberant and dreamlike | 4 с |
| `sfx/child_call.ogg` | сон («Мама?») | A small child's voice calling softly 'mama' from far away, echoing, dreamlike | 2 с |
| `sfx/katyk_pour.ogg` | флешбэк `flashback_katyk` | Milk poured into a wooden tub, a gentle splash | 3 с |
| `sfx/embroidery_pull.ogg` | флешбэк `flashback_embroidery` | A thread pulled through fabric, soft stitch | 2 с |
| `sfx/women_laugh.ogg` | флешбэк | A warm group of women laughing softly, a child laughing | 4 с |

---

## 4. Интерфейс

Короткие, моно, тихие и тёплые. Дерево, ткань и металл, не пластик.

| Файл | Когда | Промт | Длина |
|---|---|---|---|
| `ui/tap_next.ogg` | тап по реплике, дальше | Soft wooden tick, warm and quiet | 0,2 с |
| `ui/plate_in.ogg` | появление плашки | A gentle cloth swish with a faint wooden knock | 0,3 с |
| `ui/choice_show.ogg` | выезд кнопок выбора | A soft brass chime, short | 0,5 с |
| `ui/choice_pick.ogg` | выбор варианта | A warm wooden knock with a tiny metal ring | 0,3 с |
| `ui/choice_paid.ogg` | платный выбор 💎 | Gem tinkling, a small coin drop and a sparkle | 0,6 с |
| `ui/gem_spend.ogg` | трата кристаллов | Coins falling into a purse, short | 0,6 с |
| `ui/stat_up.ogg` | «+1» параметр (баннер) | A soft rising two-note kobyz pluck, warm | 0,8 с |
| `ui/stat_down.ogg` | параметр снижен | A soft falling two-note pluck, muted | 0,8 с |
| `ui/system_info.ogg` | «ОТКРЫТАЯ ИНФОРМАЦИЯ», «НОВЫЙ КВЕСТ», флаги | A magical bell with a shimmering tail, short, mystical | 1 с |
| `ui/quest_new.ogg` | новый квест «ПЕЧАТЬ» | A deep drum hit plus a bell, a sense of a secret discovered | 1,5 с |
| `ui/timer_tick.ogg` | таймер выбора | A quiet ticking of a wooden clock, loopable | 1 с |
| `ui/timer_end.ogg` | время вышло | A dull wooden thump, short | 0,5 с |
| `ui/title_in.ogg` | появление названия | A deep frame-drum hit and a bell shimmer rising, cinematic | 2 с |
| `ui/scene_change.ogg` | смена локации (затемнение) | A soft low whoosh with a wind-like tail | 0,8 с |
| `ui/loading_logo.ogg` | экран загрузки, лого | A soft fire ignition, an ember crackle and a warm rising chord | 2 с |
| `ui/episode_end.ogg` | «Продолжить» после финала | A soft kobyz note and a closing wooden knock | 1 с |

---

## 5. Голоса

Пока без озвучки, тексты показываются только плашками. Если захочется голосов: Сююмбике, Сафия, Юсуф, Джан-Али, Слуга, Женский голос (флешбэк), Мальчик (сон). Скажи, и я распишу голосовые промты и список реплик.

---

## Как подключим

Нужны: загрузка клипов (`Resources/Audio/...`), микшер с тремя шинами (музыка, амбиент, эффекты и интерфейс), кроссфейд музыки и амбиента по `# bg:` (таблица выше), воспроизведение `sfx/*` по тегам `# sfx: имя` и звуки интерфейса из экранов. Теги `# sfx` сейчас идут только текстом: я предлагаю дописать `# snd: имя` к нужным строкам.
