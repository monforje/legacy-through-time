# Промты для brand-ассетов — «Наследие. Сквозь время»

Палитра из `Theme.cs`: пергамент `#F7E9D8`, чернила `#2A1B1B`, изумруд `#157554`, алый `#E15654`, тёмный фон загрузки `#0B1F17`.

Для брендовых ассетов (сетка, splash, loader) фон — плоский `#FF00FF` (маджента), вырезаем после через `rembg`/Photoshop. Прозрачный фон напрямую генераторы дают плохо. Слайды загрузки — полноэкранные сцены, фон там настоящий.

Всего 6 промтов: 1 сетка с брендом + 3 слайда загрузки + splash + loader.

## Общий стиль (вставляется в начало каждого промта как [STYLE])

```
Hand-painted gouache and ink illustration in the manner of a historical manuscript
miniature crossed with a modern mobile-game UI kit. Warm parchment cream (#F7E9D8),
deep ink brown (#2A1B1B), emerald green (#157554), scarlet accent (#E15654), small
touches of aged gold. Soft paper grain, subtle hand-drawn ink outlines, rounded
friendly shapes, gentle drop shadow. Flat, clean, readable at small sizes. No
photorealism, no 3D render.
```

---

## Промт 1 — сетка brand-ассетов (6 штук)

Эмблема, вордмарк, app icon, favicon-глиф, notification-глиф, лоадер. Сетка 3×2, равные ячейки, без линий и рамок между ними — режем по ровной сетке.

```
[STYLE]. A brand asset sheet for a history storytelling mobile app "Наследие.
Сквозь время" (Legacy Through Time). Exactly 6 items arranged in a perfectly even
3 columns x 2 rows grid, every cell the same size, equal empty margins around each
item, NO grid lines, NO borders, NO frames, NO labels, NO captions, items never
touch or overlap. Whole sheet on one flat solid #FF00FF magenta background, no
shadows or gradients on the background. 3:2 landscape.

Row 1, left to right:
1. EMBLEM: an open antique book whose pages unroll into a winding road that fades
   to a horizon, a small sun-disc above it. Bold single silhouette, thick ink
   outline, emerald + parchment + scarlet accent, no text.
2. WORDMARK: the emblem small above, then the title "Наследие" in large elegant
   calligraphic serif, ink brown with a thin gold edge, and a smaller line
   "Сквозь время" beneath. Cyrillic letters must be accurate.
3. APP ICON: a square tile with softly rounded corners, deep emerald (#157554)
   background with faint paper texture and soft radial glow, the emblem centered
   in the inner 66%, no text.

Row 2, left to right:
4. FAVICON GLYPH: ultra-simplified emblem (book + road), solid ink brown shapes
   only, no gradients, no fine detail, must stay recognizable at 16x16.
5. NOTIFICATION GLYPH: the same simplified emblem as a single solid white
   silhouette, flat, no outline detail, no inner shading.
6. LOADER: a small ornamental hourglass with falling sand, inside a thin circular
   ornament of leaves and dots (manuscript border style), ink brown and gold,
   perfectly symmetrical, designed to rotate or pulse.
```

---

## Промты 2–4 — слайды экрана загрузки

Полноэкранные сцены на весь экран, 9:16, с полноценным фоном и окружением (не вырезки). Внизу сцена мягко темнеет к `#0B1F17`, чтобы поверх читались индикатор загрузки и подсказка; в верхней трети остаётся воздух. Текста в кадре нет. Прозрачность не нужна.

Общая приставка для сцен вместо маджентового фона:

```
Full-bleed vertical scene, 9:16, painted edge to edge, rich environment and depth,
atmospheric lighting. The bottom 25% darkens softly into deep green-black (#0B1F17)
for UI overlay. No text, no frame, no border, no UI.
```

### Промт 2 — стол в Казанском ханстве

```
[STYLE]. Full-bleed vertical scene, 9:16, painted edge to edge. Interior of a
richly decorated tent-palace of the Kazan Khanate, 16th century: three people in
embroidered robes and caps sitting cross-legged on layered carpets around a low
table with bowls of food, flatbread and tea, one pouring from a copper jug. Carved
wooden screen, hanging lamps, patterned textiles, an arched window with a glimpse
of domes and minarets at dusk. Historically accurate clothing and objects. Warm
lamplight, deep emerald and scarlet textiles, golden dust in the air. The bottom
25% darkens softly into deep green-black (#0B1F17) for UI overlay. No text, no
frame, no UI.
```

### Промт 3 — девушка на траве

```
[STYLE]. Full-bleed vertical scene, 9:16, painted edge to edge. A young woman in
an embroidered kalfak cap and long dress sitting on grass at the edge of the
steppe at golden hour, doing embroidery on her lap, a clay jug and a basket beside
her, a saddled horse grazing close behind. Endless rolling Volga steppe, wildflowers
in the foreground, a distant village with a wooden mosque minaret, big sky with
long warm clouds. Medieval, historically accurate clothing. Soft golden light from
the right, long shadows on the grass. The bottom 25% darkens softly into deep
green-black (#0B1F17) for UI overlay. No text, no frame, no UI.
```

### Промт 4 — Николай II до событий

Без сцены расстрела (казнь реальных людей, среди них дети: риск модерации сторов и перекос тона). Кадр до событий, сдержанный.

```
[STYLE]. Full-bleed vertical scene, 9:16, painted edge to edge. Tsar Nicholas II
in a plain military greatcoat and peaked cap, seen from behind and slightly from
the side, hands behind his back, standing at a tall window of a modest wooden
house in Tobolsk, 1917, looking out at a snowy street and church domes. Simple
room: wooden floor, a samovar and a family photograph on a table, a child's coat
on a chair. Dignified, quiet, melancholic mood, desaturated palette with warm
lamplight inside, cold blue light outside, one thin scarlet accent. The bottom
25% darkens softly into deep green-black (#0B1F17) for UI overlay. No text, no
frame, no UI.
```

---

## Промт 5 — Splash

Большое лого по центру. В Unity ставится на сплошной `#0B1F17` (либо на тёмный фон-фото из онбординга), поэтому генерим только лого с орнаментом.

```
[STYLE]. Splash screen hero logo for a history storytelling app. Large centered
emblem: an open antique book whose pages unroll into a winding road that fades to
a horizon, a sun-disc above it, surrounded by a soft circular halo of fine gold
manuscript ornament (leaves, dots, thin rays) that fades out at the edges. Below
the emblem the title "Наследие" in large calligraphic serif, parchment cream with
a thin gold edge, and a smaller line "Сквозь время" beneath. Designed to sit on a
very dark green (#0B1F17) screen, so lettering and ornament are light and glowing.
Cyrillic letters must be accurate. Flat solid #FF00FF magenta background, no
shadows on the background, 9:16 portrait, composition in the center 60%.
```

## Промт 6 — Loader (слои для анимации)

Отдельный большой лоадер: два раздельных слоя, чтобы крутить кольцо и пульсировать песочные часы независимо.

```
[STYLE]. Loader artwork, two separate elements side by side on one sheet, no
labels, no lines between them, equal margins:
Left: a thin circular ornamental ring (manuscript border: leaves, dots, small
diamonds), perfectly symmetrical, empty in the middle, ink brown and gold.
Right: a small antique hourglass with falling sand, parchment glass, emerald
frame, one scarlet accent on the sand, perfectly symmetrical, vertically centered.
Both on one flat solid #FF00FF magenta background, no shadows on the background,
2:1 landscape, each element centered in its half.
```

В Unity: кольцо вращаем (`RectTransform`), часы слегка пульсируют по scale.

---

## После генерации

- Сетку режем на 6 равных частей, фон `#FF00FF` вырезаем в прозрачный.
- App icon для iOS должен быть непрозрачным; для Android — foreground (эмблема, прозрачный) + background (однотонный изумруд).
- Favicon и notification-глиф лучше векторизовать (Figma/Potrace), а не ресайзить растр.
- Splash кладём на `#0B1F17`; лоадер режем на два слоя (кольцо и часы).
