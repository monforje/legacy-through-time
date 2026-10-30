#!/usr/bin/env bash
# Screens of the real UI while the editor has the project open: the sources go to a copy of the project
# (~/ltt-shots, made once with rsync, its Library included) and UiShots runs there in batchmode.
# -> ~/ltt-shots/Builds/ui-shots/blender/*.png (1080x2340); LTT_SHOT=1080x1920 (a Huawei P10), LTT_SHOT=1600x2560 LTT_DPI=224 (a tablet)
#    -> ~/ltt-shots/Builds/ui-shots-<size>/blender/*.png
set -e
SRC="$(cd "$(dirname "$0")/.." && pwd)"; DST="${LTT_SHOTS:-$HOME/ltt-shots}"
rsync -a "$SRC/Assets/Scripts/" "$DST/Assets/Scripts/"
rsync -a "$SRC/Assets/Resources/" "$DST/Assets/Resources/"
OUT="Builds/ui-shots${LTT_SHOT:+-$LTT_SHOT}"; rm -rf "$DST/$OUT"
cd "$DST"
timeout 900 "$HOME/Unity/Hub/Editor/6000.6.3f1/Editor/Unity" -batchmode -projectPath . -executeMethod LegacyThroughTime.Editor.UiShots.Run -logFile shots.log >/dev/null 2>&1 || true
rg -n 'error CS|Shell icon missing|Exception' shots.log | head -10 || true
ls "$OUT/blender" | wc -l
