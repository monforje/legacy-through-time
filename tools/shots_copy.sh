#!/usr/bin/env bash
# Screens of the real UI while the editor has the project open: the sources go to a copy of the project
# (~/ltt-shots, made once with rsync, its Library included) and UiShots runs there in batchmode.
# -> ~/ltt-shots/Builds/ui-shots/blender/*.png (1080x2340)
set -e
SRC="$(cd "$(dirname "$0")/.." && pwd)"; DST="${LTT_SHOTS:-$HOME/ltt-shots}"
rsync -a "$SRC/Assets/Scripts/" "$DST/Assets/Scripts/"
rsync -a "$SRC/Assets/Resources/" "$DST/Assets/Resources/"
rm -rf "$DST/Builds/ui-shots"
cd "$DST"
timeout 900 "$HOME/Unity/Hub/Editor/6000.6.3f1/Editor/Unity" -batchmode -projectPath . -executeMethod LegacyThroughTime.Editor.UiShots.Run -logFile shots.log >/dev/null 2>&1 || true
rg -n 'error CS|Shell icon missing|Exception' shots.log | head -10 || true
ls Builds/ui-shots/blender | wc -l
