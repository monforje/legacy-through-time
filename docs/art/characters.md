# Персонажи (Sprites): промты-сетки

Игра в стиле «Клуба романтики»: **спрайты** (этот файл) ставятся поверх **фона** ([backgrounds.md](backgrounds.md)) между диалогами. Спрайт = один человек, **PNG с прозрачным фоном (без фона, без бекграунда)**.

## Как генерируем: сетками

Одна генерация = **одна картинка 16:9 в максимальном разрешении (цель 3840×2160)**, внутри **сетка 2×2 из четырёх панелей 16:9** (1920×1080 каждая), **без фона, с прозрачностью**. Всего **13 генераций = 52 спрайтов**. Сетка героя = один человек в четырёх эмоциях, чтобы лицо и одежда совпали.

```
┌──────────┬──────────┐
│ панель 1 │ панель 2 │   16:9 (3840×2160)
├──────────┼──────────┤
│ панель 3 │ панель 4 │   без фона, PNG
└──────────┴──────────┘
```

- **Прозрачный фон.** PNG с альфа-каналом, без фона, пола и тени. Вшито в каждый промт.
- **Одна хорошая картинка** на сетку: генерируешь, выбираешь лучшую. Вшито в каждый промт.
- **Запас для обрезки.** Фигура занимает ~90% высоты панели и стоит по центру, по бокам широкое пустое поле: потом режем в 2:3 и подрезаем лишнее.
- **Стиль:** фотореализм (кадр из исторического фильма), общий с фонами. Живая кожа, волосы, ткань; не рисунок, не аниме, не кукольная кожа.

Если генератор не умеет настоящую прозрачность (большинство отдаёт RGB):
1. Добавь к промту: `on a flat solid light-grey #D0D0D0 background in every panel and gutter, no gradient, no floor, no shadow`.
2. Вырежи фон: `uvx --from 'rembg[cli]' rembg i panel.png out.png`.
3. Проверь края на тёмном и светлом фоне. Серый, а не зелёный, потому что одежда героини зелёная.

## Нарезка и сборка

```
uv run --with pillow python -I - <<'EOF'
from PIL import Image
im = Image.open('grid.png').convert('RGBA'); W, H = im.size; w, h = W // 2, H // 2
names = ['neutral', 'smile', 'sad', 'angry']          # имена панелей 1..4 из списка сетки
for n, (x, y) in zip(names, [(0, 0), (w, 0), (0, h), (w, h)]):
    p = im.crop((x, y, x + w, y + h))
    p = p.crop(p.getbbox())                              # обрезать прозрачные поля
    p.save(f'{n}.png')
EOF
```

Все спрайты кладём в `Assets/Resources/Art/Characters/<файл>.png`. Лучше при обрезке сохранять одну и ту же высоту холста для одного героя (чтобы голова не прыгала между эмоциями): сначала `getbbox()` по объединению всех панелей героя, потом одинаковая обрезка.

## Как пользоваться

1. **Порядок:** сначала `grid_suyumbike_01` (с референсом лица). Из неё берёшь панель `suyumbike_court.png` и дальше прикладываешь её как референс ко всем остальным сеткам Сююмбике.
2. Остальные герои (Сафия, Юсуф, Джан-Али) — по одной первой сетке, а во второй сетке прикладывай лучшую панель из первой.
3. Промты на английском, заметки на русском.

## Общие блоки (уже вшиты в каждую сетку, для справки)

**Формат сетки**

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.
```

**Прозрачный фон и спрайт**

```
TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.
```

**Визуальный стиль**

```
VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.
```

**Негатив**

```
NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

**Эмоции**

| Ключ | По-русски | В промте |
|---|---|---|
| `neutral` | нейтральная | calm, neutral, mouth closed, composed gaze at the viewer |
| `smile` | улыбка | a warm genuine smile, relaxed eyes |
| `laugh` | смех | laughing openly, eyes crinkled, head slightly tilted |
| `sad` | грусть | sad and hurt, eyes lowered and glistening, a small downturned mouth |
| `angry` | гнев | controlled anger, a hard stare, tight jaw, brows pulled together |
| `afraid` | страх | fear and alarm, wide eyes, tense parted lips |
| `surprised` | удивление | surprise, raised brows, wide eyes, slightly parted lips |
| `thoughtful` | задумчивость | thoughtful, looking off to the side, a slight frown |
| `determined` | решимость | resolute, chin up, steady direct gaze, firm mouth |
| `cold` | холодный взгляд | cold, hard, unreadable stare, a faint contempt |
| `smirk` | усмешка | a sly half-smile on one side of the mouth, one brow slightly raised |

---

## Герои: 11 сеток

### Сетка 1. Сююмбике: парадный наряд, 1 (`grid_suyumbike_01`)

> Главная сетка. Лицо героини берём из присланного референса. Для остальных сеток Сююмбике прикладывай и референс, и отобранную панель `suyumbike_court.png`.

- Панель 1 (top-left): `suyumbike_court.png` — нейтральная
- Панель 2 (top-right): `suyumbike_court_smile.png` — улыбка
- Панель 3 (bottom-left): `suyumbike_court_sad.png` — грусть
- Панель 4 (bottom-right): `suyumbike_court_angry.png` — гнев

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly).

PANEL 1 (top-left): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (top-right): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (bottom-left): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: sad and hurt, eyes lowered and glistening, a small downturned mouth.

PANEL 4 (bottom-right): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: controlled anger, a hard stare, tight jaw, brows pulled together.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 2. Сююмбике: парадный наряд, 2 (`grid_suyumbike_02`)

- Панель 1 (top-left): `suyumbike_court_afraid.png` — страх
- Панель 2 (top-right): `suyumbike_court_surprised.png` — удивление
- Панель 3 (bottom-left): `suyumbike_court_thoughtful.png` — задумчивость
- Панель 4 (bottom-right): `suyumbike_court_determined.png` — решимость

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly).

PANEL 1 (top-left): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: fear and alarm, wide eyes, tense parted lips.

PANEL 2 (top-right): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: surprise, raised brows, wide eyes, slightly parted lips.

PANEL 3 (bottom-left): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

PANEL 4 (bottom-right): outfit: a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil; pose: upright, chin slightly raised, hands folded at the waist. FACIAL EXPRESSION: resolute, chin up, steady direct gaze, firm mouth.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 3. Сююмбике: дорожный наряд, 1 (`grid_suyumbike_03`)

> Лагерь, повозка, дорога.

- Панель 1 (top-left): `suyumbike_travel.png` — нейтральная
- Панель 2 (top-right): `suyumbike_travel_smile.png` — улыбка
- Панель 3 (bottom-left): `suyumbike_travel_laugh.png` — смех
- Панель 4 (bottom-right): `suyumbike_travel_thoughtful.png` — задумчивость

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly).

PANEL 1 (top-left): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (top-right): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (bottom-left): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: laughing openly, eyes crinkled, head slightly tilted.

PANEL 4 (bottom-right): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 4. Сююмбике: дорожный наряд, 2 (`grid_suyumbike_04`)

- Панель 1 (top-left): `suyumbike_travel_sad.png` — грусть
- Панель 2 (top-right): `suyumbike_travel_afraid.png` — страх
- Панель 3 (bottom-left): `suyumbike_travel_angry.png` — гнев
- Панель 4 (bottom-right): `suyumbike_travel_surprised.png` — удивление

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly).

PANEL 1 (top-left): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: sad and hurt, eyes lowered and glistening, a small downturned mouth.

PANEL 2 (top-right): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: fear and alarm, wide eyes, tense parted lips.

PANEL 3 (bottom-left): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: controlled anger, a hard stare, tight jaw, brows pulled together.

PANEL 4 (bottom-right): outfit: a travel outfit, embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head; pose: relaxed, one hand lightly holding the other arm. FACIAL EXPRESSION: surprise, raised brows, wide eyes, slightly parted lips.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 5. Сююмбике: плащ (побег) и ночная рубаха (`grid_suyumbike_05`)

> Верхняя строка — плащ, нижняя — ночная рубаха (пробуждение, покои ночью).

- Панель 1 (top-left): `suyumbike_cloak_afraid.png` — страх
- Панель 2 (top-right): `suyumbike_cloak_determined.png` — решимость
- Панель 3 (bottom-left): `suyumbike_night.png` — нейтральная
- Панель 4 (bottom-right): `suyumbike_night_sad.png` — грусть

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly).

PANEL 1 (top-left): outfit: the travel outfit under a dark hooded travel cloak, hood down, hair slightly loose; pose: tense, one hand gripping the cloak at the chest. FACIAL EXPRESSION: fear and alarm, wide eyes, tense parted lips.

PANEL 2 (top-right): outfit: the travel outfit under a dark hooded travel cloak, hood down, hair slightly loose; pose: tense, one hand gripping the cloak at the chest. FACIAL EXPRESSION: resolute, chin up, steady direct gaze, firm mouth.

PANEL 3 (bottom-left): outfit: a simple light embroidered night shirt, braids loosened and falling over her shoulders; pose: soft, arms relaxed. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 4 (bottom-right): outfit: a simple light embroidered night shirt, braids loosened and falling over her shoulders; pose: soft, arms relaxed. FACIAL EXPRESSION: sad and hurt, eyes lowered and glistening, a small downturned mouth.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 6. Сафия, 1 (`grid_safiya_01`)

- Панель 1 (top-left): `safiya.png` — нейтральная
- Панель 2 (top-right): `safiya_smile.png` — улыбка
- Панель 3 (bottom-left): `safiya_laugh.png` — смех
- Панель 4 (bottom-right): `safiya_sad.png` — грусть

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SAFIYA, a young maidservant of about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful.

PANEL 1 (top-left): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (top-right): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (bottom-left): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: laughing openly, eyes crinkled, head slightly tilted.

PANEL 4 (bottom-right): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: sad and hurt, eyes lowered and glistening, a small downturned mouth.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 7. Сафия, 2 (`grid_safiya_02`)

- Панель 1 (top-left): `safiya_afraid.png` — страх
- Панель 2 (top-right): `safiya_surprised.png` — удивление
- Панель 3 (bottom-left): `safiya_thoughtful.png` — задумчивость
- Панель 4 (bottom-right): `safiya_smirk.png` — усмешка

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): SAFIYA, a young maidservant of about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful.

PANEL 1 (top-left): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: fear and alarm, wide eyes, tense parted lips.

PANEL 2 (top-right): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: surprise, raised brows, wide eyes, slightly parted lips.

PANEL 3 (bottom-left): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

PANEL 4 (bottom-right): outfit: pose: modest, hands clasped in front. FACIAL EXPRESSION: a sly half-smile on one side of the mouth, one brow slightly raised.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 8. Юсуф-бий (`grid_yusuf_01`)

> Улыбки почти нет. `cold` — тихая угроза.

- Панель 1 (top-left): `yusuf.png` — нейтральная
- Панель 2 (top-right): `yusuf_angry.png` — гнев
- Панель 3 (bottom-left): `yusuf_cold.png` — холодный взгляд
- Панель 4 (bottom-right): `yusuf_thoughtful.png` — задумчивость

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; a dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous.

PANEL 1 (top-left): outfit: pose: standing straight, hands on the sabre belt. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (top-right): outfit: pose: standing straight, hands on the sabre belt. FACIAL EXPRESSION: controlled anger, a hard stare, tight jaw, brows pulled together.

PANEL 3 (bottom-left): outfit: pose: standing straight, hands on the sabre belt. FACIAL EXPRESSION: cold, hard, unreadable stare, a faint contempt.

PANEL 4 (bottom-right): outfit: pose: standing straight, hands on the sabre belt. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 9. Джан-Али, 1 (`grid_jan_ali_01`)

- Панель 1 (top-left): `jan_ali.png` — нейтральная
- Панель 2 (top-right): `jan_ali_smile.png` — улыбка
- Панель 3 (bottom-left): `jan_ali_sad.png` — грусть
- Панель 4 (bottom-right): `jan_ali_angry.png` — гнев

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): JAN-ALI, the young khan of Kazan, about 19: slim, tense posture, pale refined tired face, thin early beard, dark eyes; a crimson-and-gold brocade caftan, jewelled belt, tall jewelled fur-trimmed cap; richly dressed but not at ease.

PANEL 1 (top-left): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (top-right): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (bottom-left): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: sad and hurt, eyes lowered and glistening, a small downturned mouth.

PANEL 4 (bottom-right): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: controlled anger, a hard stare, tight jaw, brows pulled together.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 10. Джан-Али, 2 (`grid_jan_ali_02`)

- Панель 1 (top-left): `jan_ali_afraid.png` — страх
- Панель 2 (top-right): `jan_ali_surprised.png` — удивление
- Панель 3 (bottom-left): `jan_ali_thoughtful.png` — задумчивость
- Панель 4 (bottom-right): `jan_ali_cold.png` — холодный взгляд

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): JAN-ALI, the young khan of Kazan, about 19: slim, tense posture, pale refined tired face, thin early beard, dark eyes; a crimson-and-gold brocade caftan, jewelled belt, tall jewelled fur-trimmed cap; richly dressed but not at ease.

PANEL 1 (top-left): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: fear and alarm, wide eyes, tense parted lips.

PANEL 2 (top-right): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: surprise, raised brows, wide eyes, slightly parted lips.

PANEL 3 (bottom-left): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

PANEL 4 (bottom-right): outfit: pose: standing, hands folded in front. FACIAL EXPRESSION: cold, hard, unreadable stare, a faint contempt.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 11. Маленькая Сююмбике (флешбэки) (`grid_child_01`)

> Размер в панели такой же, как у взрослых; уменьшаем в движке (~0.6).

- Панель 1 (top-left): `suyumbike_child.png` — нейтральная
- Панель 2 (top-right): `suyumbike_child_smile.png` — улыбка
- Панель 3 (bottom-left): `suyumbike_child_laugh.png` — смех
- Панель 4 (bottom-right): `suyumbike_child_sad.png` — грусть

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

CHARACTER (the same person in all four panels): LITTLE SÜYÜMBIKÄ, about 6 years old: round cheeks, big dark curious eyes, two small dark braids, a tiny embroidered cap, a simple bright embroidered dress; the same face shape as the heroine as a child.

PANEL 1 (top-left): outfit: pose: standing, hands behind her back. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (top-right): outfit: pose: standing, hands behind her back. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (bottom-left): outfit: pose: standing, hands behind her back. FACIAL EXPRESSION: laughing openly, eyes crinkled, head slightly tilted.

PANEL 4 (bottom-right): outfit: pose: standing, hands behind her back. FACIAL EXPRESSION: sad and hurt, eyes lowered and glistening, a small downturned mouth.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

## Второстепенные: 2 сетки (четыре разных человека в одной сетке)

### Сетка 1. Второстепенные 1 (`grid_minor_01`)

- Панель 1 (top-left): `woman_a.png` — Женщина из дома (флешбэки)
- Панель 2 (top-right): `chura_timer.png` — Чура Тимер
- Панель 3 (bottom-left): `chura_bulat.png` — Чура Булат
- Панель 4 (bottom-right): `envoy_kazan.png` — Гонец из Казани

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

FOUR DIFFERENT CHARACTERS, one per panel, each in a neutral calm expression:

PANEL 1 (top-left): A Nogai woman of the household, about 45, warm, in an embroidered dress and white headscarf, a kind, tired, weathered face; pose: standing, hands in her apron.

PANEL 2 (top-right): TIMER, a Nogai chura (servant-retainer) of about 35: lean, sunburned, short dark beard, steady eyes; a plain dark-green coat with a belt, a simple felt hat; pose: standing respectfully, head slightly bowed.

PANEL 3 (bottom-left): BULAT, a palace chura (servant) of Kazan, about 45: stocky, heavy-lidded, sharp moustache, a strict irritated expression; a dark-green palace coat with a red sash, a small cap; pose: standing stiff, hands behind his back.

PANEL 4 (bottom-right): A Kazan envoy rider, about 30: tired, dusty, hard face, a green travel coat with a red sash, a felt hat, a leather satchel strap across the chest; pose: standing, holding a folded letter in one hand.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. All four characters are drawn at the same scale and with the same lighting. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Сетка 2. Второстепенные 2 (`grid_minor_02`)

- Панель 1 (top-left): `chura_idris.png` — Чура Идрис
- Панель 2 (top-right): `guard_kazan.png` — Страж дворца
- Панель 3 (bottom-left): `stable_hand.png` — Конюх
- Панель 4 (bottom-right): `boy_dream.png` — Мальчик (сон)

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9). It is a 2x2 GRID of FOUR separate, complete pictures, each panel exactly 16:9 (1920x1080 each), laid out edge to edge: panel 1 top-left, panel 2 top-right, panel 3 bottom-left, panel 4 bottom-right. The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cropped out and used on its own.

FOUR DIFFERENT CHARACTERS, one per panel, each in a neutral calm expression:

PANEL 1 (top-left): IDRIS, a palace chura (servant) of Kazan, about 20: thin, nervous, big eyes, clean-shaven; a dark-green palace coat with a red sash, a small cap; pose: standing, shoulders hunched, hands wringing a cloth.

PANEL 2 (top-right): A Kazan palace guard, about 30: tall, impassive face, trimmed moustache; a dark-green coat with a red sash, a felt helmet-cap, a spear held upright in one hand; pose: standing at attention.

PANEL 3 (bottom-left): A Nogai stable hand, about 25: wiry, sunburned, short cropped hair, a plain brown felt coat and leather belt, rough hands, a startled shouting expression; pose: one arm raised pointing sharply.

PANEL 4 (bottom-right): A little boy of about 4 in a plain white shirt, dark hair, trusting eyes; pose: standing, one hand slightly raised as if holding someone's hand.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and veil edges stay clean against transparency. Each panel contains exactly ONE character, a standing 3/4 figure from the top of the head to just below the knees, centred in the panel, front-to-slight-3/4 view facing slightly to the viewer's right, about 90% of the panel height, a wide even empty margin on both sides (the panel is later cropped to 2:3). Soft even front lighting with a gentle warm key, the same in every panel, so the sprites fit any background. The face is large, sharp and readable, hands visible and natural. All four characters are drawn at the same scale and with the same lighting. Generate ONE single best image of the grid.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool felt, silk, brocade, hammered silver, aged oak, leather, copper, clay, wet earth, dust), true-to-life proportions and natural colours. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: deep forest green and vermilion-red accents against warm cream and ochre light, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: 16th-century Tatar-Nogai steppe and Kazan Khanate, year 1533. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, hammered metal jewelry with real reflections, no airbrushed or doll-like skin. Only things that really existed then: felt yurts and tents, timber and oak-and-earth fortifications with some white stone, timber and stone buildings with plain tent roofs, low seating on rugs and cushions (no chairs or tables as in Europe), oil lamps, candles and torches (no glass bulbs, no electricity), mica or small paned windows, copper and clay vessels, wooden bowls, hand-woven carpets, hand-forged iron, hand-stitched embroidery. Real period tradition of Tatar-Nogai clothing and ornament; no costume-party fantasy, no Ottoman or Arab or Russian-boyar mix-ups. Recurring motifs where natural: red rhombus (diamond) ornament, Tatar tulip embroidery.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```
