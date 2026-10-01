# Промты для фонов-сцен (с героями)

Сгенерировано под сцены `content/stories/golden-cage/episode-01.ink`. Имя файла = id из тега `# bg:`, кладём в `Assets/Resources/Art/Backgrounds/`.

## Как пользоваться

1. В каждом блоке ниже ПОЛНЫЙ промт: сцена + персонажи + композиция + стиль + негатив. Копируешь блок целиком, ничего склеивать не надо.
2. Для консистентности героини прикладывай к каждой генерации референс Сююмбике (картинку, которую ты прислал) как image-reference. Остальных персонажей после первой удачной генерации тоже прикладывай как референс.
3. Формат 9:16, лучше 1080×1920 и больше, PNG. S23 (≈9:19.5) обрежется по бокам, MatePad (3:5) сверху/снизу, поэтому главное держим в центре.
4. Промты на английском: генераторы так понимают точнее. Заметки на русском.
5. `black` рисовать не надо, это просто чёрный экран.

## Общий промт: визуальный стиль
```
VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.
```

## Общий промт: композиция (стиль «Клуба романтики»)
```
COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.
```

## Общий негатив
```
NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```

## Список файлов

1. `title.png` — Заставка истории (title)

2. `steppe_dawn.png` — Сцена 1. Ногайская Орда, рассвет

3. `tent_entrance.png` — У входа в женскую палату

4. `heroine_room_dusk.png` — Героиня просыпается в сумерках (лагерь)

5. `yusuf_hall.png` — Сцена 2. Юсуф в зале

6. `camp_yard_evening.png` — Сцена 3. Двор, вечер, сборы

7. `camp_backyard.png` — Ветка побега: задний двор

8. `cart_road.png` — В повозке на дороге

9. `road_evening.png` — Сцена 4. Поломка повозки

10. `village_house_night.png` — Ночь в деревенском доме

11. `dream_tower.png` — Вещий сон: башня

12. `village_house_dawn.png` — Пробуждение на рассвете

13. `kazan_panorama.png` — Сцена 5. Казань, общий план

14. `palace_corridor.png` — Коридор дворца

15. `heroine_room_night.png` — Комната героини в Казани, ночь

16. `khan_chamber.png` — Сцена 6. Покои хана

17. `heroine_room_evening.png` — Финал: комната героини, вечер


---


## 1. Заставка истории (title)

**Файл:** `title.png` → `Assets/Resources/Art/Backgrounds/title.png`


> Это ПОЛНОЭКРАННЫЙ фон вместо title-land.png (раньше была полоска внизу) — в TitleCard.cs потом поменяем на растяжку на весь экран. Героиня здесь одна, правило «правая треть» можно не соблюдать, центр держим пустым.


```
SCENE: STORY TITLE SCREEN BACKGROUND for 'The Golden Cage', Episode 1. A wide steppe at golden sunset, tall feather grass moving in the wind, felt tents and thin smoke far away, and on the distant horizon the white towers and walls of Kazan in warm haze. In the lower third, Suyumbike seen half from behind in 3/4 profile, braids blowing in the wind, a long veil lifted by the wind, looking toward the distant city. The CENTRE of the image, from 20% to 60% of the height, is an open calm sky with soft red-gold clouds and a large low pale sun, left clean because the story title in ornamented brackets will be placed there. Mood: epic, fateful, quiet. Slightly darker vignette at the edges.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 2. Сцена 1. Ногайская Орда, рассвет

**Файл:** `steppe_dawn.png` → `Assets/Resources/Art/Backgrounds/steppe_dawn.png`


```
SCENE: Nogai horde camp at dawn, 1533. Low sun cutting across the steppe horizon, dry grass swaying, a camp of felt-covered tents and yurts, stacked bundles, wooden chests, horse tack, a cauldron over a fire with thin smoke, horses grazing, a few people at work far in the background. In the right foreground Suyumbike stands in 3/4 profile on a small rise at the edge of the camp, hair and veil moving in the wind, her face thoughtful and uncertain, looking at the horizon. Cool blue shadows with warm ochre-red light on the grass.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 3. У входа в женскую палату

**Файл:** `tent_entrance.png` → `Assets/Resources/Art/Backgrounds/tent_entrance.png`


```
SCENE: Morning in the Nogai camp, entrance of the women's tent: a large white felt tent with red and green ornamental bands around the door curtain. Suyumbike stands on the right, just stepping out of the tent, one hand holding the door curtain, looking at Safiya with a guarded, slightly challenging expression. On the left Safiya, head respectfully lowered then raised, holds folded clothes in both arms: an embroidered shirt, a light camisole and a kalfak cap on top. Wind moves the tent cloth. A wooden chest and a rug beside the entrance.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 4. Героиня просыпается в сумерках (лагерь)

**Файл:** `heroine_room_dusk.png` → `Assets/Resources/Art/Backgrounds/heroine_room_dusk.png`


```
SCENE: Interior of Suyumbike's tent room in the Nogai camp at dusk. Felt walls with ornament, low bed with embroidered cushions and a fur blanket, a small bronze mirror on a chest, a folded kalfak lying on the chest, a low table with a copper jug, a window opening where the last orange-purple dusk light enters, dust in the beam. Suyumbike has just woken: she sits up on the bed on the right, hair loose over her shoulder, drowsy and worried, looking toward the light. No other characters. Quiet, melancholy, warm dark interior.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a simple light embroidered night shirt, braids loosened.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 5. Сцена 2. Юсуф в зале

**Файл:** `yusuf_hall.png` → `Assets/Resources/Art/Backgrounds/yusuf_hall.png`


```
SCENE: Interior of Yusuf-biy's large hall in the camp, late afternoon light (works for both afternoon and evening versions). Felt and carpet walls, a low wooden table with a small wooden bowl of katyk (thick sour milk), a copper pitcher, a lamp, servants stepping aside at the doorway, wooden pillars with carved patterns. Yusuf stands on the left, side-on at the table, his hand pushing the bowl toward his daughter, an iron gaze. Suyumbike on the right, seated or standing by the table, holding the bowl, her chin up, eyes fixed on her father, a tense silent standoff. Warm light from the entrance, dark corners.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 6. Сцена 3. Двор, вечер, сборы

**Файл:** `camp_yard_evening.png` → `Assets/Resources/Art/Backgrounds/camp_yard_evening.png`


```
SCENE: Camp yard in the evening, departure in a hurry. Several covered wagons packed and ready, nervous horses shifting their feet, servants tying wooden chests with ropes, torches being lit, long shadows, red evening sky behind felt tents. Safiya on the left holds a bundle, turned toward the heroine, anxious and gentle: 'it's time'. Suyumbike on the right, silent, in travel clothes and a dark cloak, looks away at the open steppe, hand tightening on her cloak. Dust in the air, tension.

CHARACTERS: SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 7. Ветка побега: задний двор

**Файл:** `camp_backyard.png` → `Assets/Resources/Art/Backgrounds/camp_backyard.png`


```
SCENE: The back of the Nogai camp behind the service buildings: wooden sheds, hay stacks, a stacked wood pile, drying felt, a fence, a stable corner, evening dusk. Suyumbike presses herself against the wall of a shed on the right, glancing back over her shoulder with fear and defiance, breathing hard, cloak half over her head. In the far left background a stable hand with a torch is just noticing something. Strong sense of hiding and being hunted, deep shadows, rim light from torches.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 8. В повозке на дороге

**Файл:** `cart_road.png` → `Assets/Resources/Art/Backgrounds/cart_road.png`


```
SCENE: Inside a covered travelling wagon on a road, daytime. A small wooden interior, rugs, cushions, a bundle and a small cloth pouch of dice, an open window showing a wide landscape: fields, villages, a river crossing, merchants and carts on the road, the first signs of the Kazan khanate. Safiya on the left sits laughing and holds a few bone dice in her palm, teasing. Suyumbike on the right, leaning toward the window, laughing for the first time, then her gaze drifting outside, thoughtful. Warm sunlight through the window, dust motes.

CHARACTERS: SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 9. Сцена 4. Поломка повозки

**Файл:** `road_evening.png` → `Assets/Resources/Art/Backgrounds/road_evening.png`


```
SCENE: A rutted, broken road in the evening. A covered wagon has stopped with a broken wheel and axle part, servants kneeling to repair it, ropes and tools on the ground, horses held by riders, Yusuf-biy on horseback on the left watching from a distance with a grim face, far-off lights of a village in the dusk. Suyumbike on the right has stepped out of the wagon, holding a leather strap in her hands to help; Safiya behind her, sarcastic and tired. Low red sun, long shadows, cool blue dusk coming.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 10. Ночь в деревенском доме

**Файл:** `village_house_night.png` → `Assets/Resources/Art/Backgrounds/village_house_night.png`


```
SCENE: Interior of a small wooden village house at night. Log walls, a clay hearth with embers glowing, a smoke hole, drying herbs on strings, wooden bowls and milk jugs, rugs on the floor. Suyumbike lies on the right on a low bed with a rug, head on a cushion, eyes just closing, face lit by the warm hearth glow. On the left Safiya already asleep wrapped in a blanket. Warm orange firelight against deep blue-black shadows, very intimate.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a simple light embroidered night shirt, braids loosened.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 11. Вещий сон: башня

**Файл:** `dream_tower.png` → `Assets/Resources/Art/Backgrounds/dream_tower.png`


```
SCENE: A prophetic dream. Suyumbike stands on the top of a tall stone tower on the right; below her Kazan lies strangely silent and empty, its walls without people, only wind and drifting cloud swirls. On the left a little boy holds her hand. The sky is darkening with a red storm glow, the first flames and smoke rising on the horizon, the river shining like a silver ribbon. Her hair and veil fly in the wind, fear and tenderness on her face. Dreamlike: soft blurred edges, floating ash and red sparks, desaturated cool blue-grey with intense red fire accents. Still realistic and detailed.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). a little boy of about 4 in a plain white shirt, dark hair, trusting eyes. Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 12. Пробуждение на рассвете

**Файл:** `village_house_dawn.png` → `Assets/Resources/Art/Backgrounds/village_house_dawn.png`


```
SCENE: The same small wooden village house as the night version, now at first dawn. Cold blue-grey light through a small window, the hearth is only ash, mist outside. Suyumbike sits up on the right, breathing unevenly, looking at her open palm where her own fingernail has left a mark, shaken and thoughtful. On the left Safiya sleeps peacefully under a blanket. Soft pale morning light, quiet, unsettled mood.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a simple light embroidered night shirt, braids loosened.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 13. Сцена 5. Казань, общий план

**Файл:** `kazan_panorama.png` → `Assets/Resources/Art/Backgrounds/kazan_panorama.png`


> Здесь героиня небольшая, на первом плане — повозка и дорога; город — главный.


```
SCENE: Wide establishing view of Kazan in the 1530s from the road: a white-stone and timber fortress on a hill with strong walls and towers, minarets and mosque domes, dense town with wooden houses and streets, a river with boats, a pier, a busy road in the foreground with merchants, carts, riders and cattle, a covered wagon passing in the right foreground. Suyumbike's face and shoulders in profile in the wagon window on the right, watching the city with complicated feelings. No later Russian-style architecture. Morning light with ochre haze, banners in red and green.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 14. Коридор дворца

**Файл:** `palace_corridor.png` → `Assets/Resources/Art/Backgrounds/palace_corridor.png`


```
SCENE: Interior palace corridor of the Kazan khan's house: high walls with carved wooden beams, thick carpets, low chests, copper vessels, fabrics and cushions, one small window letting in a shaft of daylight. A servant bursts out of a side door on the left and nearly collides with the heroine. Suyumbike on the right has just stepped back and is catching her balance on the edge of a carpet, Safiya grabbing her arm, with a sarcastic look. Two servants hurry in the background carrying chests. The same image is used also for the next morning scene, so keep the light neutral-morning.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 15. Комната героини в Казани, ночь

**Файл:** `heroine_room_night.png` → `Assets/Resources/Art/Backgrounds/heroine_room_night.png`


> Одна и та же комната в трёх вариантах (ночь / вечер): держите планировку окна и кровати одинаковой.


```
SCENE: Suyumbike's bedchamber in the Kazan palace at night: carved wooden window with latticework, rich carpets on walls and floor, a low bed with embroidered cushions, a brass lamp, a chest with ornament, moonlight on the floor. She sits by the window on the right, in the glow of a small oil lamp, holding a small painted miniature on a wooden board and tracing the wax seal on its back with a finger, thoughtful and wary. No other characters. Moody, secretive, deep blues with a warm lamp pool.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a simple light embroidered night shirt, braids loosened.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 16. Сцена 6. Покои хана

**Файл:** `khan_chamber.png` → `Assets/Resources/Art/Backgrounds/khan_chamber.png`


```
SCENE: The khan's chamber in the Kazan palace, morning. Carved wooden ceiling, carpets, brass candlesticks, low divan with silk cushions, a tall window with soft daylight. Jan-Ali stands on the left by the window, half-turned to face her, young, tense but with a surprised faint smile. Suyumbike on the right, bowing her head slightly then lifting her proud, direct eyes to him, in a kalfak and a deep-green embroidered caftan. Charged silence between them, a clear romantic and political tension.

CHARACTERS: JAN-ALI, the young khan of Kazan, about 19: slim, tense posture, pale refined tired face, thin early beard, dark eyes; crimson-and-gold brocade caftan, jewelled belt, tall jewelled fur-trimmed cap; richly dressed but not at ease. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 17. Финал: комната героини, вечер

**Файл:** `heroine_room_evening.png` → `Assets/Resources/Art/Backgrounds/heroine_room_evening.png`


```
SCENE: The same Kazan palace bedchamber as in the night version, now in the late evening: the window is open, the sky outside burning red-orange, the first stars, soft golden light on the carpets. Suyumbike sits alone on the right by the window looking out toward where the steppe lies, a resolved, quietly determined expression; the door on the left is closed and the shadow of a guard's feet is visible under it. Wind moves the curtain. A poised, hopeful but heavy mood.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```
