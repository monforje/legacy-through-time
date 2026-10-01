# Session screen elements: shapes for asset generation

Source: `session-screens.html` + `skeleton.css`. Every element in the skeleton is a grey or black
placeholder. This file describes the **shape** of each one so that art can be generated
to replace it. Colors below are skeleton values, not final art.

## Conventions

- **Reference frame:** Galaxy S23 portrait, 360×780 CSS px. Export at **@3x** (1080×2340).
  All sizes below are CSS px; multiply by 3 for the export.
- **Two surface tones** run through the whole UI:
  - **Light surface**: plates and panels (skeleton `#f4f4f4`, 2 px outline `#111`).
  - **Dark surface**: labels, buttons, banners (skeleton `#2b2b2b`, white text, no outline).
- **Stretching:** almost every container changes width (tablet) and height (text length).
  Generate them as **9-slice** sprites: decorated corners, plain edges and center
  that can stretch or tile. The 9-slice margin for each element is given below.
- **Text is not part of the art.** All copy is rendered by the engine on top of the sprites.
- Content inside every block is vertically centered.

## Asset list

| # | Asset | Base size (CSS px) | 9-slice | Screens |
|---|-------|--------------------|---------|---------|
| 1 | Dialogue plate | 320 × ≥110 | yes, 12 | 1–8, 10, 12 |
| 1a | Thought plate | 320 × ≥110 | yes, 26 | 6a |
| 2 | Speech tail | 40 × 30 | no | 4–8, 10 |
| 2a | Thought bubbles | 18 × 18 + 10 × 10 | no | 6a |
| 3 | Label (name tag) | ≥110 × 30 | yes, 8 | all with a plate or panel |
| 4 | Option button | 320 × ≥46 | yes, 8 | 7, 7a, 8, 11 |
| 5 | Gem icon | 18 × 18 | no | 7, 9, 9a |
| 6 | Timer ring | 50 × 50 | no | 8 |
| 7 | Stat banner | 320 × ≥50 | yes, 8 | 10 |
| 8 | Stat icon | 28 × 28 | no | 10 |
| 9 | Picker panel | 320 × ~210 | yes, 12 | 9, 9a |
| 10 | Arrow button | 36 × 48 | yes, 8 | 9, 9a |
| 11 | Arrow chevron | 10 × 10 | no | 9, 9a |
| 12 | Confirm button ("Выбрать.") | 288 × 50 | yes, 8 | 9, 9a |
| 13 | Collapse chevron | 20 × 11 | no | 9, 9a, 11 |
| 14 | Gem balance pill | ≥130 × 44 | yes, 8 | 9a |
| 15 | Plus glyph | 16 × 16 | no | 9a |
| 16 | Choice panel | 320 × variable | yes, 12 | 11 |
| 17 | Info cloud | 280 × ≥120 | yes, 12 | 12 |
| 18 | Info badge | 40 × 40 | no | 12 |
| 19 | Title brackets | 320 × 3 (×2) | yes, horizontal | 0 |
| 20 | Dim overlay | full screen | no | 0 |

---

## 1. Dialogue plate

The main text box. One shape for every kind of text: narration, fact, hint, speech, thought.

- **Shape:** wide horizontal rectangle with softly rounded corners (radius 10).
  Light fill, 2 px dark outline.
- **Size:** full width minus 20 px side margins (320 wide). Minimum height 110, grows upward
  with text. The bottom edge always sits on the same line, about 32% of screen height from
  the bottom (250 px).
- **Padding:** 24 top, 18 sides and bottom. The label (#3) overlaps the top edge, so the top
  padding is larger.
- **9-slice:** 12 px on all sides. Edges must stretch or tile cleanly, because the plate
  grows in both directions.
- **Variants:** narration, fact and hint differ only by label text. Speech adds the tail (#2).
  Thought uses its own plate, see #1a.

## 1a. Thought plate (screen 6a)

The heroine's inner voice. Same placement and size as the dialogue plate, a different outline.

- **Shape:** same rectangle, but with much rounder corners (radius 24), so it reads as a soft
  cloud next to the sharper speech plate. Light fill, 2 px dark outline. No quotes in the text:
  the shape already marks it as a thought.
- **Label:** heroine's name tag on the left, same as speech (#3).
- **9-slice:** 26 px (the corners are larger).

## 2. Speech tail

A small pointer that makes the plate a speech bubble, aimed at the speaking character drawn
on the background.

- **Shape:** a **slanted triangle** standing on the top edge of the plate. The base lies on
  the plate edge; the apex is pushed sideways, so the tail leans like a comic-book pointer.
  - Box 40 × 30. Base is 22 px wide (from 45% to 100% of the box width); apex at the top
    outer corner of the box.
  - Same fill and 2 px outline as the plate. The outline follows both slanted sides and
    merges into the plate outline with no seam; the base has no outline (it is open to the
    plate).
- **Placement:** 70 px in from the plate side, on the side **opposite** the label.
  - Label on the left (heroine) → tail on the right, leaning **left**.
  - Label on the right (everyone else) → tail on the left, leaning **right** (mirror image).
- **Deliverable:** one sprite; the mirrored one is a horizontal flip. Bottom 3 px of the sprite
  should be plate fill, so it covers the plate outline where they join.

## 2a. Thought bubbles

Replace the speech tail on a thought plate: the classic comic-book trail of small circles.

- **Shape:** two circles with the plate's fill and 2 px outline, not touching the plate or each
  other:
  - near circle 18 × 18, its bottom 8 px above the plate top, 70 px in from the plate's right edge;
  - far circle 10 × 10, higher (bottom ~34 px above the plate top) and 18 px further left.
  The trail rises and leans toward the screen center, like the heroine's tail.
- **Deliverable:** two sprites, or one sprite with both circles (box ~46 × 44). Mirrored for a
  right-side speaker by a horizontal flip.

## 3. Label (name tag)

A small dark tab sitting on the top edge of a plate or panel. Shows the speaker's name, or the
block type ("…", "Факт", "Подсказка"), or the action in a choice block.

- **Shape:** short rounded rectangle (radius 6), dark fill, no outline.
- **Size:** minimum 110 × 30, grows horizontally with text, may wrap to two lines.
  In the picker it is 78% of the panel width, in the choice panel 70%.
- **Placement:** straddles the top edge of its parent: about half above, half on the plate
  (top at -16 px). Three positions: centered, left (22 px from plate edge), right (22 px).
- **9-slice:** 8 px.
- **Text:** white, 15 px, centered. Italic in the picker.

## 4. Option button

One answer in a choice list. Options stack at the bottom of the screen and overlap the
bottom of the plate.

- **Shape:** wide rounded rectangle (radius 6), dark fill, no outline.
- **Size:** same width as the plate (320), shifted 12 px to the right: indented 12 px on the
  left, sticks out 12 px past the plate on the right. In the mirror layout (7a) it is
  shifted left instead. Minimum height 46, grows for two-line answers. Gap between options 10.
- **Overlap:** the first option covers the bottom 16 px of the plate.
- **Inside:** text left-aligned, 18 px side padding. Paid options have a price at the right
  edge: number + gem icon (#5).
- **9-slice:** 8 px.
- **States:**
  - normal: dark;
  - pressed: lighter dark (`#555`), scaled to 97%;
  - chosen: inverted, light fill with a 2 px dark inner outline, dark text;
  - rejected: same as normal at 40% opacity.

## 5. Gem icon

The paid currency. Replaces the 💎 emoji everywhere.

- **Shape:** faceted gem seen from the front: flat top (table), angled crown facets, pointed
  bottom (pavilion). Reads clearly at small size on a dark background.
- **Size:** about 18 × 18 (matches 17 px text). Export @3x, 54 × 54.
- **Used in:** option price, confirm button price, balance pill.

## 6. Timer ring

Countdown for a timed choice.

- **Shape:** circle 50 × 50 with a 4 px dark ring. Inside, a dark **pie** that drains
  clockwise from full to empty as time runs out; the drained part shows plate color.
- **Placement:** above the top-right corner of the plate (8 px in from the right, 70 px above
  the plate top), so it does not cover the label.
- **Deliverables:** ring frame and a full disc for the fill (the engine masks it as a radial
  fill). In the last 3 seconds the whole timer pulses (scale to 114%).

## 7. Stat banner

A notification that a stat changed ("+1 Память степи"). Drops in from the top and hides itself
after 3.5 s or on tap. The plate below does not move.

- **Shape:** wide rounded rectangle (radius 6), dark fill, no outline.
- **Size:** 320 × ≥50. Top sits 24 px below the status bar.
- **Inside:** stat icon (#8) at the left, 14 px from the edge; text centered with 52 px padding
  on each side, so it stays centered next to the icon.
- **9-slice:** 8 px.

## 8. Stat icon

- **Shape:** square slot 28 × 28. One icon per stat (for example "Память степи": a steppe
  motif). In the skeleton it is a dashed square.
- **Style:** light line art on dark background, same as banner text color.

## 9. Picker panel (choice with arrows)

A single block at the bottom of the screen: the label states the action, one value is shown
between two arrows, and the confirm button is below.

- **Shape:** wide rectangle (radius 10), light fill, 2 px dark outline. Same look as the plate.
- **Size:** 320 wide, 34 px above the screen bottom. Height follows content, about 210:
  36 top padding (for the label), arrow row 64, gap 14, confirm button 50, 16 bottom padding.
- **Layout of the row:** arrow button 36, gap 8, value text (22 px, centered, up to two lines),
  gap 8, arrow button 36.
- **Attached elements:** label (#3) on top edge, centered, 78% width, italic;
  collapse chevron (#13) above the label; in 9a also the balance pill (#14) above the
  right corner.
- **9-slice:** 12 px.
- **Behavior:** slides down off the screen when collapsed, leaving only the chevron; slides
  back up on tap. Also enters the screen this way.

## 10. Arrow button

- **Shape:** tall rounded rectangle 36 × 48 (radius 6), dark fill, no outline.
- **States:** pressed is lighter (`#555`) and scaled to 90%; disabled is 40% opacity.
- **9-slice:** 8 px (height stays fixed, so a plain sprite works too).

## 11. Arrow chevron

- **Shape:** open chevron, two strokes meeting at a right angle, stroke 3 px, white.
  Fits a 10 × 10 box (corner of an "L" rotated 45°). Points left on "previous", right on
  "next" (one sprite, flipped).
- Optically centered: nudged 2 px toward the back of the chevron, so the point does not look
  off-center.

## 12. Confirm button ("Выбрать.")

- **Shape:** wide rounded rectangle (radius 6), dark fill, no outline. Full inner width of the
  picker (288) × 50.
- **Inside:** "Выбрать." left-aligned, 18 px padding. If the current value is paid, a price
  (number + gem icon) sits at the right edge.
- **States:** same as option button (#4): pressed, chosen (inverted). On not enough gems it
  shakes horizontally.
- **9-slice:** 8 px.

## 13. Collapse chevron

Toggle that hides and shows a choice panel.

- **Shape:** a solid **V-shaped band** (not a stroke): 20 wide × 11 tall, band thickness about
  a third of the height (~3.5 px), symmetric. Dark color, no background.
- **Placement:** centered, 60 px above the panel top. Tap area 48 × 40.
- **States:** "V" (pointing down) when the panel is open; rotates 180° to "^" when collapsed.
  One sprite, rotated by the engine; it must look identical when rotated around its center.

## 14. Gem balance pill

Shows the current gem balance above a paid picker.

- **Shape:** rounded rectangle (radius 6), dark fill, no outline. Height 44, minimum width 130.
- **Placement:** right-aligned to the picker, 82 px above its top edge.
- **Inside, left to right:** gem icon (#5), balance number, then a plus (#15) pushed to the
  right edge. 14 px side padding, 12 px between items.
- **9-slice:** 8 px horizontally.

## 15. Plus glyph

- **Shape:** plus sign, white, about 16 × 16, stroke matching text weight (~3 px). Tappable:
  opens buying gems. Pressed state is 60% opacity.

## 16. Choice panel (choice without a speech bubble)

Options in their own block at the bottom, introduced by a label phrase
("Сафия ждёт ответа…").

- **Shape:** same frame as the picker panel (#9): radius 10, light fill, 2 px dark outline.
- **Size:** 320 wide, 30 px above the screen bottom. Height = 36 top padding + options
  (46 each, gap 10) + 16 bottom padding.
- **Inside:** option buttons (#4) at full inner width (288), text left-aligned, not shifted.
- **Attached elements:** label (#3) centered on the top edge, 70% width; collapse chevron (#13).
- **9-slice:** 12 px. If the picker and choice panel share one sprite, that is fine.

## 17. Info cloud

Explanation of a term ("Калфак — …") at the top of the screen. Closes on tap.

- **Shape:** rectangle with radius 10, light fill, 2 px dark outline (the plate look).
  If a distinct look is wanted, a softer "cloud" outline is allowed, but text must still fit a
  rectangle of the given size.
- **Size:** 280 wide (40 px margins), minimum height 120, grows with text. Top is 16 px below
  the status bar. Text centered, 20–22 px padding.
- **9-slice:** 12 px.

## 18. Info badge

- **Shape:** dark circle 40 × 40 with a bold white "!" in the middle.
- **Placement:** centered on the top-right corner of the info cloud (offset -16, -16), so it
  hangs half outside.

## 19. Title brackets

Frame for the story and scene title on screen 0.

- **Shape:** two horizontal **brackets**, one above and one below the title text. In the
  skeleton they are plain white lines 3 px thick. The final art is meant to be ornamental:
  a line with a decorated center and/or ends, with the ends turning slightly toward the text
  like a wide "[" rotated 90°. The bottom bracket is the top one flipped vertically.
- **Size:** 320 wide each. Top bracket at 26% of screen height (~203 px), bottom at 65%
  (~507 px). The text block sits centered between them: story name 28 px bold, scene 22 px,
  28 px apart.
- **Color:** white over the dimmed background.
- **9-slice:** horizontal only; the ornamental center and ends are fixed, the line between
  them stretches.
- **Animation:** opens from the center outward vertically, so each bracket should read well
  on its own.

## 20. Dim overlay

- Black at 35% opacity over the whole background on the title screen. No sprite needed;
  a solid color fill is enough.

---

## Not in the art set

- Screen backgrounds with characters (the green placeholder). Those are a separate art set.
- Status bar and gesture bar. System UI; only their space is reserved (32 px top, 24 px bottom).
- Text. All copy is rendered by the engine.
