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

18. `kalfak_embroidery.png` — Крупный план: вышивка калфака

19. `bronze_mirror.png` — Сююмбике у бронзового зеркала

20. `flashback_embroidery.png` — Флешбэк: девочка среди вышивальщиц

21. `katyk_bowl.png` — Чаша с катыком

22. `flashback_katyk.png` — Флешбэк: женщины переливают молоко

23. `yusuf_stare.png` — Юсуф смотрит прямо на дочь

24. `yusuf_table_slam.png` — Юсуф бьёт ладонью по столу

25. `kazan_envoys.png` — Гонцы из Казани

26. `yusuf_letter.png` — Юсуф читает письмо

27. `wagon_departure.png` — Отъезд повозок из лагеря

28. `camp_search.png` — Поиски по лагерю

29. `stable_catch.png` — Конюх замечает беглянку

30. `yusuf_confronts.png` — Юсуф встречает беглянку

31. `locked_room.png` — Запертая комната

32. `knife_closeup.png` — Дорожный нож

33. `dice_game.png` — Игра в кости в повозке

34. `toll_road.png` — Дорога с заставой

35. `wagon_repair_help.png` — Сююмбике помогает чинить повозку

36. `village_lights.png` — Огни деревни

37. `dream_fire.png` — Сон: башня в огне

38. `palm_closeup.png` — Ладонь со следом от ногтя

39. `kazan_gate.png` — Въезд в Казань

40. `miniature_closeup.png` — Миниатюра с печатью

41. `servants_search.png` — Слуги ищут печать

42. `overheard_servants.png` — Подслушанный разговор

43. `wax_seal.png` — Печать на обороте

44. `khan_face_to_face.png` — Хан и Сююмбике лицом к лицу

45. `khan_silence.png` — Молчание в покоях хана

46. `khan_by_window.png` — Хан и Сююмбике у окна

47. `chamber_door_guard.png` — Стража у двери покоев


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


> Это одна и та же комната, что в heroine_room_evening.png (ночь и вечер): держите планировку окна и кровати одинаковой.


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


## 18. Крупный план: вышивка калфака

**Файл:** `kalfak_embroidery.png` → `Assets/Resources/Art/Backgrounds/kalfak_embroidery.png`


> Только руки Сююмбике и Сафии в кадре, лица не нужны.


```
SCENE: Extreme close-up of the kalfak cap held by Safiya's hands and touched by Suyumbike's fingers: dense red, green and cream embroidery of tulips and rhombus ornaments on dark green velvet, tiny silver coins and beads, visible stitches and thread texture. Soft daylight from a tent opening, a blurred felt tent wall behind, a faint feeling of memory and nostalgia.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 19. Сююмбике у бронзового зеркала

**Файл:** `bronze_mirror.png` → `Assets/Resources/Art/Backgrounds/bronze_mirror.png`


```
SCENE: Interior of Suyumbike's tent room. She stands on the right in front of a small polished bronze mirror on a carved chest, putting on the tall embroidered kalfak cap, her face seen in profile and slightly in the mirror's dim reflection, thoughtful, melancholic. A shaft of soft light through the tent door, rugs and cushions, a copper jug. Intimate and quiet.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 20. Флешбэк: девочка среди вышивальщиц

**Файл:** `flashback_embroidery.png` → `Assets/Resources/Art/Backgrounds/flashback_embroidery.png`


```
SCENE: FLASHBACK memory, warm hazy soft-focus look with a faded golden glow and slightly desaturated edges, like a remembered dream. Inside a felt tent, little Suyumbike sits among Nogai women who are embroidering fabric by hand; one woman adjusts the child's tiny cap, another holds a thread and laughs gently. The child holds a thread too tight, a piece of embroidered cloth on her lap. Girl on the right, women on the left. Dust motes in golden light.

CHARACTERS: LITTLE SÜYÜMBIKÄ, about 6 years old: round cheeks, big dark curious eyes, two small dark braids, a tiny embroidered cap, a simple bright embroidered dress; the same face shape as the heroine, as a child. Nogai women of the household (2-3 of them), warm, middle-aged, in embroidered dresses and white headscarves, with kind, tired, weathered faces. 

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 21. Чаша с катыком

**Файл:** `katyk_bowl.png` → `Assets/Resources/Art/Backgrounds/katyk_bowl.png`


> Только руки Юсуфа и героини.


```
SCENE: Close-up of a low wooden table in Yusuf-biy's hall: a small carved wooden bowl of thick white katyk (sour milk) with a wooden spoon, a copper pitcher, a flat bread. A man's heavy hand with a ring slides the bowl across the table toward a young woman's slender hand on the right. Warm lamp light, shallow depth of field, the blurred green-and-red felt wall behind. Tension in the gesture.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 22. Флешбэк: женщины переливают молоко

**Файл:** `flashback_katyk.png` → `Assets/Resources/Art/Backgrounds/flashback_katyk.png`


```
SCENE: FLASHBACK memory, warm hazy soft-focus look with a faded golden glow, like a remembered dream. Nogai women pour fresh milk from a big wooden vessel into a large wooden tub with white foam, in a sunlit yurt entrance. Little Suyumbike sits close on the right with a wooden spoon, asking something with a curious face, one woman on the left looks at her with a thoughtful, tender expression. Steam, light dust, warm gold.

CHARACTERS: LITTLE SÜYÜMBIKÄ, about 6 years old: round cheeks, big dark curious eyes, two small dark braids, a tiny embroidered cap, a simple bright embroidered dress; the same face shape as the heroine, as a child. Nogai women of the household (2-3 of them), warm, middle-aged, in embroidered dresses and white headscarves, with kind, tired, weathered faces. 

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 23. Юсуф смотрит прямо на дочь

**Файл:** `yusuf_stare.png` → `Assets/Resources/Art/Backgrounds/yusuf_stare.png`


```
SCENE: Tight two-shot in Yusuf-biy's hall: Yusuf on the left, in profile-3/4, looking straight at his daughter for the first time with a heavy, hurt and hard gaze; Suyumbike on the right, holding the wooden bowl, returning his stare with a lifted chin. Faces close to each other, a strong feeling of silence. Warm lamp light on the faces, dark felt wall behind.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 24. Юсуф бьёт ладонью по столу

**Файл:** `yusuf_table_slam.png` → `Assets/Resources/Art/Backgrounds/yusuf_table_slam.png`


```
SCENE: The moment of anger. Yusuf-biy on the left slams his palm on the low table, the bowl of katyk jumps and white drops splash in the air, his face dark with fury and pain, mouth open as he shouts. Suyumbike on the right leans back slightly, shocked but not breaking, eyes wide, bowl still in her hand. Dynamic frozen moment, dramatic side light.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 25. Гонцы из Казани

**Файл:** `kazan_envoys.png` → `Assets/Resources/Art/Backgrounds/kazan_envoys.png`


```
SCENE: The yard of the Nogai camp at the edge of evening: six riders from Kazan on tired, foaming horses arrive in a cloud of dust, in dark green and crimson coats with the Kazan banners, one rider holding out a sealed letter. On the left a servant in the doorway shouts in alarm; on the right Suyumbike stands in the hall's door, one hand on the door frame, watching with sudden unease. Red low sun, dust, tension.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 26. Юсуф читает письмо

**Файл:** `yusuf_letter.png` → `Assets/Resources/Art/Backgrounds/yusuf_letter.png`


```
SCENE: Close shot of Yusuf-biy on the left reading a letter with a broken wax seal, his face turning cold and hard, the paper held in a tense fist; on the right Suyumbike watching his face with a sharp, suspicious look, half in shadow. Dim hall, one oil lamp lighting the letter and his face from below. Dramatic and quiet.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 27. Отъезд повозок из лагеря

**Файл:** `wagon_departure.png` → `Assets/Resources/Art/Backgrounds/wagon_departure.png`


```
SCENE: Evening departure. A caravan of covered wagons rolls out of the Nogai camp with riders, felt tents and a column of smoke behind them in the red dusk. The view is from inside the lead wagon window: on the right Suyumbike, in profile, looks back at the shrinking camp with a restrained sorrow, one hand touching the window frame; on the left Safiya sits beside her with her hands in her lap, silent. Warm dusk light, dust.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 28. Поиски по лагерю

**Файл:** `camp_search.png` → `Assets/Resources/Art/Backgrounds/camp_search.png`


```
SCENE: A night search in the Nogai camp: riders and servants running with torches between felt tents and wagons, shouting, horses rearing, long jumping shadows and orange torchlight against a deep blue night. In the foreground on the right Suyumbike, partially hidden behind a stack of chests, holds her breath, her face lit by distant firelight. A sense of chase and fear.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 29. Конюх замечает беглянку

**Файл:** `stable_catch.png` → `Assets/Resources/Art/Backgrounds/stable_catch.png`


```
SCENE: Behind the stables at dusk: a stable hand with a torch on the left has just spotted Suyumbike and points at her, shouting. Suyumbike on the right in a dark travel cloak, half running, turning her face toward him in alarm, hair loose, one hand holding up her skirt. Stacked hay, wooden fence, horses looking out of the stalls. Torch glare and cold shadow.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 30. Юсуф встречает беглянку

**Файл:** `yusuf_confronts.png` → `Assets/Resources/Art/Backgrounds/yusuf_confronts.png`


```
SCENE: The Nogai camp yard at dusk, torches burning. Yusuf-biy stands silent on the left, arms behind his back, looking at his daughter with cold controlled anger; servants wait at a distance with lowered eyes. Suyumbike on the right, dishevelled, breathing hard, chin raised, defiance and guilt in her eyes. Dust on her cloak. The moment before a verdict, heavy silence.

CHARACTERS: YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 31. Запертая комната

**Файл:** `locked_room.png` → `Assets/Resources/Art/Backgrounds/locked_room.png`


```
SCENE: A small bare room in the camp used as a lock-up: a single narrow window with wooden bars letting in thin evening light, a plain rug, a low stool, a heavy door with a bar outside. Suyumbike sits on the floor on the right, back to the felt wall, knees up, braids undone, staring at the barred window with quiet anger. No other characters. Cold blue light with a thin warm stripe. Lonely, dramatic.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 32. Дорожный нож

**Файл:** `knife_closeup.png` → `Assets/Resources/Art/Backgrounds/knife_closeup.png`


> Лицо Сафии размыто на заднем плане.


```
SCENE: Close-up: Suyumbike's hand taking a small ornate travel knife with a carved bone handle and a leather sheath from a chest, her fingers closing around it; in the soft-focus background Safiya sees it, wide-eyed and worried. Dim evening light, wooden chest lid, cloth bundles. A thin rim light on the steel edge.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 33. Игра в кости в повозке

**Файл:** `dice_game.png` → `Assets/Resources/Art/Backgrounds/dice_game.png`


> Только руки и кости.


```
SCENE: Close-up on a small cloth spread over a wooden crate inside a travelling wagon: three bone dice rolling to a stop, a few coins and a handful of beads as stakes. Two pairs of hands: Safiya's on the left tossing, Suyumbike's on the right, one finger lightly resting on a die with a sly gesture. Warm sunlight through the wagon window, dust motes, the green landscape blurred in the window behind.

CHARACTERS: SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 34. Дорога с заставой

**Файл:** `toll_road.png` → `Assets/Resources/Art/Backgrounds/toll_road.png`


```
SCENE: The road into the Kazan khanate seen from the wagon window: a toll barrier of wooden poles across the road, guards with spears in green coats collecting payment from merchants, a line of carts and cattle, a small watchtower, fields cut by roads. On the right Suyumbike's profile in the foreground of the window, thoughtful and a bit afraid, the golden afternoon light behind her. 'Every road has an owner' feeling.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 35. Сююмбике помогает чинить повозку

**Файл:** `wagon_repair_help.png` → `Assets/Resources/Art/Backgrounds/wagon_repair_help.png`


```
SCENE: Dusk on the road. Servants on the left struggle with a broken wagon axle and ropes while a servant looks at Suyumbike in surprise: she has stepped down and, in her fine dress, holds a thick leather strap taut, bracing with both hands, sleeves pushed up, a determined little smile. Safiya watches from the wagon steps, amused. Red sky, long shadows, dust.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 36. Огни деревни

**Файл:** `village_lights.png` → `Assets/Resources/Art/Backgrounds/village_lights.png`


```
SCENE: A quiet blue dusk on the road. Far across a dark field the warm yellow lights of a small village glow like stars. Suyumbike on the right stands at the edge of the road with her cloak, looking longingly at the lights; Yusuf-biy on the left, on foot and in profile, blocks her with a stern raised hand, not unkind but firm. The broken wagon and servants in soft focus behind.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). YUSUF-BIY, her father, a Nogai biy of about 50: broad shoulders, weathered stern face, short grey-streaked beard and moustache, heavy brows, cold controlled gaze; dark green long fur-trimmed caftan with an embroidered belt, tall fur hat, a sabre at the belt; calm and dangerous. Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 37. Сон: башня в огне

**Файл:** `dream_fire.png` → `Assets/Resources/Art/Backgrounds/dream_fire.png`


```
SCENE: A nightmarish dream sequence. A tall stone tower with flames leaping from its windows, walls of the fortress cracking, black smoke, red sky, a river reflecting fire far below. In the foreground Suyumbike reaches desperately toward the little boy whose small hand is slipping from her fingers; the boy's face is partly blurred and turned away. Floating sparks and ash. Realistic but dreamlike, red-and-black palette with cold blue shadows.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). a little boy of about 4 in a plain white shirt, dark hair, trusting eyes. Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 38. Ладонь со следом от ногтя

**Файл:** `palm_closeup.png` → `Assets/Resources/Art/Backgrounds/palm_closeup.png`


> Лицо Сююмбике размыто на заднем плане.


```
SCENE: Close-up of Suyumbike's open palm in the first cold dawn light, with a red crescent mark left by her own fingernail; her blurred worried face in the background, the other hand trembling slightly. The cold blue light from a small window, a rug and the blurred sleeping Safiya far behind. Very intimate and quiet.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 39. Въезд в Казань

**Файл:** `kazan_gate.png` → `Assets/Resources/Art/Backgrounds/kazan_gate.png`


```
SCENE: The wagon passes through the great gate of Kazan: thick white-stone walls and a tall gate tower with banners, guards in green coats, a crowd of townspeople, merchants with baskets, children running, loud colourful city life. View from inside the wagon: on the right Suyumbike at the window looking up at the gate with a complicated look, a curtain edge in the blurred foreground. Warm morning light, dust and smoke.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a travel outfit: embroidered cream shirt, light deep-green camisole with red tulip embroidery, silver coin necklace, braids over her shoulders, bare head or a light veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 40. Миниатюра с печатью

**Файл:** `miniature_closeup.png` → `Assets/Resources/Art/Backgrounds/miniature_closeup.png`


> Лицо Сафии размыто на заднем плане.


```
SCENE: Close-up of Suyumbike's hands holding a small painted miniature portrait on a wooden board: a man in rich dark-red clothes and a tall jewelled hat, fine gold-leaf details; the other hand turning it to show a wax seal on the back. The blurred palace corridor and Safiya's concerned face behind. Soft side light from a window, rich detail on paint and wood.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 41. Слуги ищут печать

**Файл:** `servants_search.png` → `Assets/Resources/Art/Backgrounds/servants_search.png`


```
SCENE: A side room of the Kazan palace seen from the doorway: two servants in a panic search through open chests, throwing out fabrics, cushions and papers; one grabs the other by the collar and argues. Suyumbike on the right stands in the doorway holding the miniature behind her back, watching them with narrowed eyes and a calm face; Safiya on the left behind her, anxious. Warm lamp light, chaos of cloth and copper.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). SAFIYA, her young maidservant, about 17: round friendly face, quick half-smile, light-brown eyes, one long brown braid under a simple cream-and-madder-red headscarf, modest ochre-brown dress with an apron, clean but humble clothes; loyal, witty, playful. Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 42. Подслушанный разговор

**Файл:** `overheard_servants.png` → `Assets/Resources/Art/Backgrounds/overheard_servants.png`


```
SCENE: A palace gallery with carved wooden columns and silk curtains. Two servants on the left whisper urgently to each other, glancing around nervously. Suyumbike on the right hides behind a curtain and a column, only half of her face and one eye visible, listening intently. A lamp in the middle glows, long shadows. Tension, secrets.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 43. Печать на обороте

**Файл:** `wax_seal.png` → `Assets/Resources/Art/Backgrounds/wax_seal.png`


> Лицо героини размыто.


```
SCENE: Close-up: Suyumbike's finger traces a small red wax seal on the back of a wooden miniature board; the seal shows an unfamiliar crest with a tulip and a crescent. The room is dark, only a small oil lamp and moonlight; her thoughtful face is out of focus in the glow. Rich texture of wax, wood and fabric of her sleeve.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). 

COMPOSITION (Romance Club style visual-novel scene, detail shot): vertical 9:16 full-bleed illustration, a close or macro shot, the key object or hands sharp in the upper-middle band of the frame, shallow depth of field, background softly blurred. The bottom 35% of the frame is calmer and darker (cloth, table, shadow) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple. Keep important details away from the left and right edges (the image gets cropped on different screens).

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 44. Хан и Сююмбике лицом к лицу

**Файл:** `khan_face_to_face.png` → `Assets/Resources/Art/Backgrounds/khan_face_to_face.png`


```
SCENE: A tight two-shot in the khan's chamber: Jan-Ali on the left stepped close to Suyumbike, tense and urgent but vulnerable; Suyumbike on the right facing him with a hard, challenging stare, their faces close. Strong window light from one side, a carved wooden wall behind. Emotional charge, fury and attraction at once.

CHARACTERS: JAN-ALI, the young khan of Kazan, about 19: slim, tense posture, pale refined tired face, thin early beard, dark eyes; crimson-and-gold brocade caftan, jewelled belt, tall jewelled fur-trimmed cap; richly dressed but not at ease. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 45. Молчание в покоях хана

**Файл:** `khan_silence.png` → `Assets/Resources/Art/Backgrounds/khan_silence.png`


```
SCENE: The khan's chamber in a heavy silence. Suyumbike on the right turns her face away from the khan with a closed expression, eyes on the carpet, hands folded; Jan-Ali on the left looks at her, waiting, a trace of hurt in his tired eyes. A long ray of light on the floor between them, dust in the air. Distance in the composition: a visible empty space between them.

CHARACTERS: JAN-ALI, the young khan of Kazan, about 19: slim, tense posture, pale refined tired face, thin early beard, dark eyes; crimson-and-gold brocade caftan, jewelled belt, tall jewelled fur-trimmed cap; richly dressed but not at ease. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 46. Хан и Сююмбике у окна

**Файл:** `khan_by_window.png` → `Assets/Resources/Art/Backgrounds/khan_by_window.png`


```
SCENE: A quieter moment in the khan's chamber: Jan-Ali on the left and Suyumbike on the right stand side by side at a tall carved window, looking at the city of Kazan below, not at each other, a shared tired honesty. Soft golden morning light on their faces, a ribbon of the river in the distance. The mood is calm and tentatively warm.

CHARACTERS: JAN-ALI, the young khan of Kazan, about 19: slim, tense posture, pale refined tired face, thin early beard, dark eyes; crimson-and-gold brocade caftan, jewelled belt, tall jewelled fur-trimmed cap; richly dressed but not at ease. SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```


## 47. Стража у двери покоев

**Файл:** `chamber_door_guard.png` → `Assets/Resources/Art/Backgrounds/chamber_door_guard.png`


```
SCENE: A narrow corridor outside Suyumbike's chamber: a guard in a dark-green coat with a spear stands in front of the closed carved door on the left, impassive, blocking it. Suyumbike on the right in the door's gap, hand gripping the wood, looking at the guard with controlled anger. A lamp on the wall, long shadows on the carpets. A feeling of being kept inside.

CHARACTERS: SÜYÜMBIKÄ (Syuyumbike), a 17-year-old Nogai princess (khanbike): slender, fair olive skin, large dark expressive eyes, straight dark brows, very long thick dark braids, proud, intelligent, restrained defiance in her face. Same face and look as the attached reference image (match it exactly). Suyumbike wears a deep-green embroidered caftan with red and cream tulip-and-rhombus patterns, silver coin necklace, a tall embroidered kalfak cap with a sheer cream veil.

COMPOSITION (Romance Club style visual-novel scene): vertical 9:16 full-bleed illustration, characters painted directly into the background, no separate cut-out feel. Camera at eye level, characters framed from knee or waist up (3/4 shot), faces large and readable, emotional acting. The heroine stands on the RIGHT third of the frame, other characters on the LEFT third; the centre stays open. Faces and main action sit in the upper-middle band of the frame. The bottom 35% of the frame is calmer and slightly darker (soft shadow, floor, grass, table, fabric) because dialogue boxes and choice buttons will be placed over it. The top 10% is simple (sky, ceiling, wall) for a speech bubble. Keep important details away from the left and right edges (the image gets cropped on different phone and tablet screens). Shallow depth of field: a blurred foreground element, sharp characters, softly blurred background.

VISUAL STYLE: high-detail realistic digital painting with a cinematic, slightly painterly finish, like premium visual-novel key art. Historical realism: 16th-century Tatar-Nogai steppe and Kazan Khanate world, year 1533. Rich tangible detail: embroidery stitches, felt and wool texture, brocade, silver and coin jewelry, carved wood, copper vessels, worn leather, dust, smoke. Soft volumetric light, deep rich shadows, subtle film grain. Brand palette: deep forest green, coral / vermilion red, warm cream-parchment, ochre-gold sunlight, charcoal black. Recurring motifs where natural: red rhombus (diamond) folk ornament, Tatar tulip embroidery, drifting stylised smoke or cloud swirls, a hint of bonfire warmth. Faces are beautiful, expressive and believable (realistic proportions, not anime, not 3D render, not cartoon, not uncanny photo). Cohesive colour grading across the whole series: green-and-red accents against warm cream and ochre light.

NO text, NO letters, NO logo, NO watermark, NO UI elements, NO frame or border, NO modern objects, NO anime style, NO extra fingers, NO distorted faces.
```
