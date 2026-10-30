# Персонажи (Sprites): промты, ряды в полный рост. «По хребтам Сихотэ-Алиня»

Тот же формат, что в [../characters.md](../characters.md): **спрайты** ставятся поверх **фона** ([backgrounds.md](backgrounds.md)). Спрайт = один человек, **PNG с прозрачным фоном (без фона, без бекграунда)**.

## Как генерируем: рядами в полный рост

Одна генерация = **одна картинка 16:9 в максимальном разрешении (цель 3840×2160)**, внутри **ряд из четырёх узких вертикальных панелей** (по 960×2160, то есть 4:9), **без фона, с прозрачностью**. Всего **6 генераций = 24 спрайта**. Ряд героя = один человек в четырёх эмоциях, чтобы лицо и одежда совпали.

- **Прозрачный фон.** PNG с альфа-каналом, без фона, пола и тени. Вшито в каждый промт.
- **Одна хорошая картинка** на ряд: генерируешь, выбираешь лучшую. Вшито в каждый промт.
- **Узкий силуэт, в полный рост, все смотрят чуть вправо.** Игра сама отражает спрайт при необходимости.
- **Стиль:** фотореализм (кадр из исторического фильма), общий с фонами. Живая кожа, волосы, ткань; не рисунок, не аниме, не кукольная кожа.

Если генератор не умеет настоящую прозрачность:
1. Добавь к промту: `on a flat solid light-grey #D0D0D0 background in every panel and gutter, no gradient, no floor, no shadow`.
2. Вырежи фон: `uvx --from 'rembg[cli]' rembg i row.png out.png`.
3. Проверь края на тёмном и светлом фоне.

## Нарезка и сборка

Та же, что в [../characters.md](../characters.md#нарезка-и-сборка). Имена панелей слева направо берём из списка ряда.

Все спрайты кладём в `Assets/Resources/Art/Characters/<файл>.png`. Имя файла = `# char:` из `episode-01.ink` (+ `_<эмоция>`).

## Как пользоваться

1. **Порядок:** сначала `grid_volodya_01` (мальчик). Его лучшую панель прикладываем как референс только к ряду 2. Взрослый Арсеньев рисуется отдельно, без референса мальчика.
2. Мать, отец: по одному ряду, референсов не нужно.
3. **Если генератор (ChatGPT) отказывает:** в промтах нет настоящих имён и возрастов детей; мальчик нарисован живописью, остальные фотореалистично. Если всё равно отказ, уберите слова про кожу (`REAL PEOPLE: …`) или попросите весь ряд в живописном стиле, как у мальчика (блок `STYLE_PAINTED` взят из ряда 1).
3. Промты на английском, заметки на русском.
4. Дети в игре уменьшаются (~0.7), как маленькая Сююмбике: рост в панели рисуем так же, как у взрослых.

## Какие эмоции использует сценарий

| Герой | Эмоции в `.ink` | Нарисовано |
|---|---|---|
| Володя | neutral, smile, laugh, thoughtful, afraid, surprised, determined, smirk | 8 (ряды 1–2) |
| Мать | neutral, smile, angry, smirk | 4 |
| Отец | neutral, smile, laugh, thoughtful | 4 |
| Арсеньев | neutral, smile, thoughtful, determined | 4 |
| Матрос | neutral | 1 (запас лиц порта: ещё 3) |

## Общие блоки

Вшиты в каждую сетку ниже. Формат ряда, прозрачный фон и стиль см. в самих промтах, они повторяются дословно.

---

## Герои: 5 рядов

### Ряд 1. Володя, мальчик, 1 (`grid_volodya_01`)

> Главная сетка мальчика. Он нарисован живописью, а не фото: так генераторы не блокируют детей. Сначала утверди его.

- Панель 1 (panel 1 (leftmost)): `volodya.png` — нейтральная
- Панель 2 (panel 2): `volodya_smile.png` — улыбка
- Панель 3 (panel 3): `volodya_laugh.png` — смех
- Панель 4 (panel 4 (rightmost)): `volodya_thoughtful.png` — задумчивость

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9), divided into FOUR equal tall portrait panels standing SIDE BY SIDE in one row, left to right, each panel exactly 960x2160 (4:9, much taller than wide). The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture of ONE full-length figure: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cut out and used on its own.

CHARACTER (the same character in all four panels): A FICTIONAL CHARACTER, a young schoolboy of 1880s Saint Petersburg, shown as a storybook-style painted figure: a lively, curious expression, slightly messy dark-brown hair, bright attentive dark eyes, a small blue ink smudge on his fingers.

PANEL 1 (panel 1 (leftmost)): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (panel 2): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (panel 3): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: laughing openly, eyes crinkled, head slightly tilted.

PANEL 4 (panel 4 (rightmost)): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and cloth edges stay clean against transparency. Each panel contains exactly ONE character. FULL-LENGTH FIGURE FROM THE TOP OF THE HEAD TO THE SOLES OF THE FEET, both feet and the shoes fully visible, standing upright on an invisible ground, the whole body inside the panel with a small margin above the head and below the feet; the figure fills about 94% of the panel height. NARROW, TALL SILHOUETTE: the person stands straight and slim like a column, shoulders square to the camera or turned slightly (front-to-slight-3/4), arms hanging relaxed CLOSE to the body or hands lightly joined in front of the waist, elbows in, no wide gestures, no spread arms, no flaring sleeves or coat skirts sticking out to the sides, so the figure is about three times taller than wide. Life-size natural proportions (head about 1/7 of the body height for an adult), neither chibi nor elongated. Eye-level camera, as if seen from a few metres away on a long lens, no perspective distortion. Soft even front lighting with a gentle warm key and soft fill, the same in every panel, so the sprites fit any background. The face is sharp and readable, the acting is in the face and the posture. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the whole row.

VISUAL STYLE: REFINED PAINTERLY PERIOD-DRAMA ILLUSTRATION, a soft realistic gouache and oil look like a high-quality illustration for a historical novel: clean confident brushwork, warm natural colours, gentle modelling of light, believable period clothing, crisp clean edges, no blur, no noise, no compression artifacts. Restrained cohesive colour grade: deep Prussian blue and brass accents on cream and oak, warm lamplight amber, charcoal shadows. HISTORICAL ACCURACY: Saint Petersburg, Russian Empire, the 1880s; period-correct children's clothing, no costume-party fantasy, no modern objects.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO anime, NO 3D render look, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Ряд 2. Володя, мальчик, 2 (`grid_volodya_02`)

> Прикладывай лучшую панель из ряда 1 как референс персонажа.

- Панель 1 (panel 1 (leftmost)): `volodya_afraid.png` — страх
- Панель 2 (panel 2): `volodya_surprised.png` — удивление
- Панель 3 (panel 3): `volodya_determined.png` — решимость
- Панель 4 (panel 4 (rightmost)): `volodya_smirk.png` — усмешка

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9), divided into FOUR equal tall portrait panels standing SIDE BY SIDE in one row, left to right, each panel exactly 960x2160 (4:9, much taller than wide). The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture of ONE full-length figure: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cut out and used on its own.

CHARACTER (the same character in all four panels): A FICTIONAL CHARACTER, a young schoolboy of 1880s Saint Petersburg, shown as a storybook-style painted figure: a lively, curious expression, slightly messy dark-brown hair, bright attentive dark eyes, a small blue ink smudge on his fingers. The same character design as in the attached image.

PANEL 1 (panel 1 (leftmost)): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: fear and alarm, wide eyes, tense parted lips.

PANEL 2 (panel 2): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: surprise, raised brows, wide eyes, slightly parted lips.

PANEL 3 (panel 3): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: resolute, chin up, steady direct gaze, firm mouth.

PANEL 4 (panel 4 (rightmost)): outfit: a dark-blue wool tunic with a leather belt and a small turned-down white collar, dark wool knee trousers, long dark stockings, scuffed leather ankle boots visible; pose: standing straight, hands behind his back, chin slightly raised, curious. FACIAL EXPRESSION: a sly half-smile on one side of the mouth, one brow slightly raised.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and cloth edges stay clean against transparency. Each panel contains exactly ONE character. FULL-LENGTH FIGURE FROM THE TOP OF THE HEAD TO THE SOLES OF THE FEET, both feet and the shoes fully visible, standing upright on an invisible ground, the whole body inside the panel with a small margin above the head and below the feet; the figure fills about 94% of the panel height. NARROW, TALL SILHOUETTE: the person stands straight and slim like a column, shoulders square to the camera or turned slightly (front-to-slight-3/4), arms hanging relaxed CLOSE to the body or hands lightly joined in front of the waist, elbows in, no wide gestures, no spread arms, no flaring sleeves or coat skirts sticking out to the sides, so the figure is about three times taller than wide. Life-size natural proportions (head about 1/7 of the body height for an adult), neither chibi nor elongated. Eye-level camera, as if seen from a few metres away on a long lens, no perspective distortion. Soft even front lighting with a gentle warm key and soft fill, the same in every panel, so the sprites fit any background. The face is sharp and readable, the acting is in the face and the posture. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the whole row.

VISUAL STYLE: REFINED PAINTERLY PERIOD-DRAMA ILLUSTRATION, a soft realistic gouache and oil look like a high-quality illustration for a historical novel: clean confident brushwork, warm natural colours, gentle modelling of light, believable period clothing, crisp clean edges, no blur, no noise, no compression artifacts. Restrained cohesive colour grade: deep Prussian blue and brass accents on cream and oak, warm lamplight amber, charcoal shadows. HISTORICAL ACCURACY: Saint Petersburg, Russian Empire, the 1880s; period-correct children's clothing, no costume-party fantasy, no modern objects.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO anime, NO 3D render look, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Ряд 3. Мать (`grid_mother_01`)

> Мать Володи, Петербург, 1880-е.

- Панель 1 (panel 1 (leftmost)): `mother.png` — нейтральная
- Панель 2 (panel 2): `mother_smile.png` — улыбка
- Панель 3 (panel 3): `mother_angry.png` — гнев
- Панель 4 (panel 4 (rightmost)): `mother_smirk.png` — усмешка

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9), divided into FOUR equal tall portrait panels standing SIDE BY SIDE in one row, left to right, each panel exactly 960x2160 (4:9, much taller than wide). The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture of ONE full-length figure: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cut out and used on its own.

CHARACTER (the same character in all four panels): A FICTIONAL CHARACTER, a mother of a family, about 35, a Saint Petersburg gentlewoman of the 1880s: a kind, tired, intelligent face with fine lines of laughter, dark hair pinned in a simple low bun, warm brown eyes, a gentle but firm bearing.

PANEL 1 (panel 1 (leftmost)): outfit: a long dark-plum wool day dress with a high collar, a small lace collar and cuffs, a modest brooch, a skirt that reaches the ankles, soft leather house shoes visible; pose: standing straight, hands lightly folded in front of the waist, elbows close to the body. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (panel 2): outfit: a long dark-plum wool day dress with a high collar, a small lace collar and cuffs, a modest brooch, a skirt that reaches the ankles, soft leather house shoes visible; pose: standing straight, hands lightly folded in front of the waist, elbows close to the body. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (panel 3): outfit: a long dark-plum wool day dress with a high collar, a small lace collar and cuffs, a modest brooch, a skirt that reaches the ankles, soft leather house shoes visible; pose: standing straight, hands lightly folded in front of the waist, elbows close to the body. FACIAL EXPRESSION: controlled anger, a hard stare, tight jaw, brows pulled together.

PANEL 4 (panel 4 (rightmost)): outfit: a long dark-plum wool day dress with a high collar, a small lace collar and cuffs, a modest brooch, a skirt that reaches the ankles, soft leather house shoes visible; pose: standing straight, hands lightly folded in front of the waist, elbows close to the body. FACIAL EXPRESSION: a sly half-smile on one side of the mouth, one brow slightly raised.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and cloth edges stay clean against transparency. Each panel contains exactly ONE character. FULL-LENGTH FIGURE FROM THE TOP OF THE HEAD TO THE SOLES OF THE FEET, both feet and the shoes fully visible, standing upright on an invisible ground, the whole body inside the panel with a small margin above the head and below the feet; the figure fills about 94% of the panel height. NARROW, TALL SILHOUETTE: the person stands straight and slim like a column, shoulders square to the camera or turned slightly (front-to-slight-3/4), arms hanging relaxed CLOSE to the body or hands lightly joined in front of the waist, elbows in, no wide gestures, no spread arms, no flaring sleeves or coat skirts sticking out to the sides, so the figure is about three times taller than wide. Life-size natural proportions (head about 1/7 of the body height for an adult), neither chibi nor elongated. Eye-level camera, as if seen from a few metres away on a long lens, no perspective distortion. Soft even front lighting with a gentle warm key and soft fill, the same in every panel, so the sprites fit any background. The face is sharp and readable, the acting is in the face and the posture. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the whole row.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool cloth, worn leather, oak parquet, brass, wet timber, hemp rope, paper, graphite, ink, damp stone, conifer needles), true-to-life proportions and natural colours. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, no airbrushed or doll-like skin. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: cool grey-blue rain light against warm kerosene-lamp amber, deep Prussian blue and brass accents on cream paper and oak, and for the Far East scenes sea-green fog, wet dark timber and the deep green of the taiga, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: Russian Empire. Saint Petersburg in the 1880s (a comfortable middle-class apartment: oak parquet, tiled stoves, heavy curtains, kerosene lamps and candles, wooden desks, steel-nib pens, inkwells, graphite pencils, hand-bound books with steel engravings, no electric light indoors) and the port of Vladivostok in 1900 (wooden piers and quays, steamships with funnels and masts, crates, barrels, hemp ropes, horse carts, timber and brick buildings on steep hills, forested ridges behind the town). Only things that really existed then; no cars, no modern ships or cranes, no concrete, no Soviet architecture, no plastic. Period-correct clothing of the Russian Empire, no costume-party fantasy.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Ряд 4. Отец (`grid_father_01`)

> Отец Володи, Петербург, 1880-е.

- Панель 1 (panel 1 (leftmost)): `father.png` — нейтральная
- Панель 2 (panel 2): `father_smile.png` — улыбка
- Панель 3 (panel 3): `father_laugh.png` — смех
- Панель 4 (panel 4 (rightmost)): `father_thoughtful.png` — задумчивость

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9), divided into FOUR equal tall portrait panels standing SIDE BY SIDE in one row, left to right, each panel exactly 960x2160 (4:9, much taller than wide). The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture of ONE full-length figure: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cut out and used on its own.

CHARACTER (the same character in all four panels): A FICTIONAL CHARACTER, a father of a family, about 42, a Saint Petersburg man of the 1880s, a modest educated professional: a calm, humorous, weathered face, a neatly trimmed dark moustache and short beard with early grey, warm attentive eyes.

PANEL 1 (panel 1 (leftmost)): outfit: a dark grey frock coat over a waistcoat with a watch chain, a white shirt with a soft collar and a dark tie, dark trousers, polished leather shoes visible; pose: standing straight, one hand in the coat pocket, the other close to the body. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (panel 2): outfit: a dark grey frock coat over a waistcoat with a watch chain, a white shirt with a soft collar and a dark tie, dark trousers, polished leather shoes visible; pose: standing straight, one hand in the coat pocket, the other close to the body. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (panel 3): outfit: a dark grey frock coat over a waistcoat with a watch chain, a white shirt with a soft collar and a dark tie, dark trousers, polished leather shoes visible; pose: standing straight, one hand in the coat pocket, the other close to the body. FACIAL EXPRESSION: laughing openly, eyes crinkled, head slightly tilted.

PANEL 4 (panel 4 (rightmost)): outfit: a dark grey frock coat over a waistcoat with a watch chain, a white shirt with a soft collar and a dark tie, dark trousers, polished leather shoes visible; pose: standing straight, one hand in the coat pocket, the other close to the body. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and cloth edges stay clean against transparency. Each panel contains exactly ONE character. FULL-LENGTH FIGURE FROM THE TOP OF THE HEAD TO THE SOLES OF THE FEET, both feet and the shoes fully visible, standing upright on an invisible ground, the whole body inside the panel with a small margin above the head and below the feet; the figure fills about 94% of the panel height. NARROW, TALL SILHOUETTE: the person stands straight and slim like a column, shoulders square to the camera or turned slightly (front-to-slight-3/4), arms hanging relaxed CLOSE to the body or hands lightly joined in front of the waist, elbows in, no wide gestures, no spread arms, no flaring sleeves or coat skirts sticking out to the sides, so the figure is about three times taller than wide. Life-size natural proportions (head about 1/7 of the body height for an adult), neither chibi nor elongated. Eye-level camera, as if seen from a few metres away on a long lens, no perspective distortion. Soft even front lighting with a gentle warm key and soft fill, the same in every panel, so the sprites fit any background. The face is sharp and readable, the acting is in the face and the posture. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the whole row.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool cloth, worn leather, oak parquet, brass, wet timber, hemp rope, paper, graphite, ink, damp stone, conifer needles), true-to-life proportions and natural colours. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, no airbrushed or doll-like skin. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: cool grey-blue rain light against warm kerosene-lamp amber, deep Prussian blue and brass accents on cream paper and oak, and for the Far East scenes sea-green fog, wet dark timber and the deep green of the taiga, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: Russian Empire. Saint Petersburg in the 1880s (a comfortable middle-class apartment: oak parquet, tiled stoves, heavy curtains, kerosene lamps and candles, wooden desks, steel-nib pens, inkwells, graphite pencils, hand-bound books with steel engravings, no electric light indoors) and the port of Vladivostok in 1900 (wooden piers and quays, steamships with funnels and masts, crates, barrels, hemp ropes, horse carts, timber and brick buildings on steep hills, forested ridges behind the town). Only things that really existed then; no cars, no modern ships or cranes, no concrete, no Soviet architecture, no plastic. Period-correct clothing of the Russian Empire, no costume-party fantasy.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

### Ряд 5. Арсеньев, взрослый (`grid_arsenyev_01`)

> Арсеньев в 1900 году, Владивосток. В промте нет настоящего имени: генераторы отказывают на реальных людей. Имя остаётся только в файлах игры.

- Панель 1 (panel 1 (leftmost)): `arsenyev.png` — нейтральная
- Панель 2 (panel 2): `arsenyev_smile.png` — улыбка
- Панель 3 (panel 3): `arsenyev_thoughtful.png` — задумчивость
- Панель 4 (panel 4 (rightmost)): `arsenyev_determined.png` — решимость

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9), divided into FOUR equal tall portrait panels standing SIDE BY SIDE in one row, left to right, each panel exactly 960x2160 (4:9, much taller than wide). The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture of ONE full-length figure: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cut out and used on its own.

CHARACTER (the same character in all four panels): A FICTIONAL CHARACTER, a young officer of the Russian Imperial Army in 1900, about 28: lean, fit, a weather-tanned determined face, dark-brown hair cut short, a neat trimmed moustache, intelligent attentive dark eyes.

PANEL 1 (panel 1 (leftmost)): outfit: a dark-green Russian army officer's tunic of 1900 buttoned to the collar with plain shoulder boards, dark trousers tucked into tall black leather boots, a leather belt with a cross strap, a peaked service cap tucked under one arm, a rolled map in the other hand held close to the body; pose: standing straight, chin slightly raised. FACIAL EXPRESSION: calm, neutral, mouth closed, composed gaze at the viewer.

PANEL 2 (panel 2): outfit: a dark-green Russian army officer's tunic of 1900 buttoned to the collar with plain shoulder boards, dark trousers tucked into tall black leather boots, a leather belt with a cross strap, a peaked service cap tucked under one arm, a rolled map in the other hand held close to the body; pose: standing straight, chin slightly raised. FACIAL EXPRESSION: a warm genuine smile, relaxed eyes.

PANEL 3 (panel 3): outfit: a dark-green Russian army officer's tunic of 1900 buttoned to the collar with plain shoulder boards, dark trousers tucked into tall black leather boots, a leather belt with a cross strap, a peaked service cap tucked under one arm, a rolled map in the other hand held close to the body; pose: standing straight, chin slightly raised. FACIAL EXPRESSION: thoughtful, looking off to the side, a slight frown.

PANEL 4 (panel 4 (rightmost)): outfit: a dark-green Russian army officer's tunic of 1900 buttoned to the collar with plain shoulder boards, dark trousers tucked into tall black leather boots, a leather belt with a cross strap, a peaked service cap tucked under one arm, a rolled map in the other hand held close to the body; pose: standing straight, chin slightly raised. FACIAL EXPRESSION: resolute, chin up, steady direct gaze, firm mouth.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and cloth edges stay clean against transparency. Each panel contains exactly ONE character. FULL-LENGTH FIGURE FROM THE TOP OF THE HEAD TO THE SOLES OF THE FEET, both feet and the shoes fully visible, standing upright on an invisible ground, the whole body inside the panel with a small margin above the head and below the feet; the figure fills about 94% of the panel height. NARROW, TALL SILHOUETTE: the person stands straight and slim like a column, shoulders square to the camera or turned slightly (front-to-slight-3/4), arms hanging relaxed CLOSE to the body or hands lightly joined in front of the waist, elbows in, no wide gestures, no spread arms, no flaring sleeves or coat skirts sticking out to the sides, so the figure is about three times taller than wide. Life-size natural proportions (head about 1/7 of the body height for an adult), neither chibi nor elongated. Eye-level camera, as if seen from a few metres away on a long lens, no perspective distortion. Soft even front lighting with a gentle warm key and soft fill, the same in every panel, so the sprites fit any background. The face is sharp and readable, the acting is in the face and the posture. The same character has the same face, outfit, proportions and size in every panel where it appears; only the facial expression changes. Generate ONE single best image of the whole row.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool cloth, worn leather, oak parquet, brass, wet timber, hemp rope, paper, graphite, ink, damp stone, conifer needles), true-to-life proportions and natural colours. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, no airbrushed or doll-like skin. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: cool grey-blue rain light against warm kerosene-lamp amber, deep Prussian blue and brass accents on cream paper and oak, and for the Far East scenes sea-green fog, wet dark timber and the deep green of the taiga, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: Russian Empire. Saint Petersburg in the 1880s (a comfortable middle-class apartment: oak parquet, tiled stoves, heavy curtains, kerosene lamps and candles, wooden desks, steel-nib pens, inkwells, graphite pencils, hand-bound books with steel engravings, no electric light indoors) and the port of Vladivostok in 1900 (wooden piers and quays, steamships with funnels and masts, crates, barrels, hemp ropes, horse carts, timber and brick buildings on steep hills, forested ridges behind the town). Only things that really existed then; no cars, no modern ships or cranes, no concrete, no Soviet architecture, no plastic. Period-correct clothing of the Russian Empire, no costume-party fantasy.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```

## Второстепенные: 1 ряд (четыре разных человека)

### Второстепенные: порт Владивостока (`grid_minor_01`)

- Панель 1 (panel 1 (leftmost)): `sailor.png` — Матрос парохода
- Панель 2 (panel 2): `porter.png` — Грузчик
- Панель 3 (panel 3): `ship_officer.png` — Пароходный офицер
- Панель 4 (panel 4 (rightmost)): `harbour_policeman.png` — Городовой порта

```
OUTPUT FORMAT: ONE single image at the highest available resolution (target 3840x2160, 16:9), divided into FOUR equal tall portrait panels standing SIDE BY SIDE in one row, left to right, each panel exactly 960x2160 (4:9, much taller than wide). The panels are separated by a thin straight gutter; NO borders, NO frames, NO captions, NO numbers, NO labels. Every panel is a fully independent picture of ONE full-length figure: nothing crosses the gutters, nothing is cut off by a panel edge, each panel has high resolution, sharp detail and clean quality, so that it can be cut out and used on its own.

FOUR DIFFERENT CHARACTERS, one per panel, each in a neutral calm expression:

PANEL 1 (panel 1 (leftmost)): A steamship sailor of 1900, about 30: broad, sunburned, short beard, steady eyes; a navy-blue sailor's jersey under a short pea jacket, a flat sailor's cap, wide trousers, heavy boots; pose: standing straight and respectful, one arm slightly raised as if pointing at a gangway, the other close to the body; boots visible.

PANEL 2 (panel 2): A Vladivostok port porter of 1900, about 40: heavy-set, bearded, tired kind eyes; a rough grey linen shirt with rolled sleeves, a leather strap over one shoulder, dark baggy trousers, worn boots; pose: standing straight, arms hanging close to the body; boots visible.

PANEL 3 (panel 3): A merchant steamship officer of 1900, about 45: grey whiskers, sharp tired eyes; a dark-blue double-breasted uniform coat with brass buttons, a peaked cap with a small badge, white shirt and black tie; pose: standing straight, hands behind his back; boots visible.

PANEL 4 (panel 4 (rightmost)): A Russian harbour policeman (gorodovoy) of 1900, about 35: tall, impassive, a full moustache; a dark-green uniform coat with white-metal buttons and a belt, a peaked cap, a sheathed sabre at the hip; pose: standing at attention, hands close to the body; boots visible.

TRANSPARENT BACKGROUND IN EVERY PANEL AND IN THE GUTTERS: PNG with an alpha channel, NO background of any kind, NO scenery, NO floor, NO ground, NO cast shadow, NO wall, NO props floating around, NO halo or glow, NO frame. Clean crisp cut-out edges without a white or coloured fringe; hair strands and cloth edges stay clean against transparency. Each panel contains exactly ONE character. FULL-LENGTH FIGURE FROM THE TOP OF THE HEAD TO THE SOLES OF THE FEET, both feet and the shoes fully visible, standing upright on an invisible ground, the whole body inside the panel with a small margin above the head and below the feet; the figure fills about 94% of the panel height. NARROW, TALL SILHOUETTE: the person stands straight and slim like a column, shoulders square to the camera or turned slightly (front-to-slight-3/4), arms hanging relaxed CLOSE to the body or hands lightly joined in front of the waist, elbows in, no wide gestures, no spread arms, no flaring sleeves or coat skirts sticking out to the sides, so the figure is about three times taller than wide. Life-size natural proportions (head about 1/7 of the body height for an adult), neither chibi nor elongated. Eye-level camera, as if seen from a few metres away on a long lens, no perspective distortion. Soft even front lighting with a gentle warm key and soft fill, the same in every panel, so the sprites fit any background. The face is sharp and readable, the acting is in the face and the posture. All four characters are drawn at the same scale and with the same lighting. Generate ONE single best image of the whole row.

VISUAL STYLE: PHOTOREALISTIC CINEMATIC REALISM, like a still frame from a high-budget historical drama shot on a large-format cinema camera with a 35-50mm lens: physically accurate light and materials, real textures (wool cloth, worn leather, oak parquet, brass, wet timber, hemp rope, paper, graphite, ink, damp stone, conifer needles), true-to-life proportions and natural colours. REAL PEOPLE: believable photographic faces with natural skin pores, fine skin texture, subtle asymmetry, real eyes with wet highlights, individual hair strands and flyaways, realistic hands, natural fabric folds and weave, no airbrushed or doll-like skin. Ultra-high resolution, extremely sharp and clean, rich micro-detail in every surface, correct perspective and architecture, crisp edges, no blur, no smudging, no oversmoothing, no AI artifacts, no noise, no compression artifacts. Restrained cohesive colour grade across the whole series: cool grey-blue rain light against warm kerosene-lamp amber, deep Prussian blue and brass accents on cream paper and oak, and for the Far East scenes sea-green fog, wet dark timber and the deep green of the taiga, charcoal shadows, natural film-like contrast. NOT an illustration, NOT a painting, NOT concept art, NOT anime, NOT cartoon, NOT 3D render, NOT plastic CGI. HISTORICAL ACCURACY: Russian Empire. Saint Petersburg in the 1880s (a comfortable middle-class apartment: oak parquet, tiled stoves, heavy curtains, kerosene lamps and candles, wooden desks, steel-nib pens, inkwells, graphite pencils, hand-bound books with steel engravings, no electric light indoors) and the port of Vladivostok in 1900 (wooden piers and quays, steamships with funnels and masts, crates, barrels, hemp ropes, horse carts, timber and brick buildings on steep hills, forested ridges behind the town). Only things that really existed then; no cars, no modern ships or cranes, no concrete, no Soviet architecture, no plastic. Period-correct clothing of the Russian Empire, no costume-party fantasy.

NO background of any kind, NO scenery, NO floor, NO shadow on the ground, NO text, NO letters, NO logo, NO watermark, NO UI, NO frame, NO modern objects, NO illustration look, NO painting look, NO anime, NO 3D render look, NO plastic or doll-like skin, NO blur, NO low resolution, NO extra fingers, NO distorted faces, NO second person in a panel.
```
