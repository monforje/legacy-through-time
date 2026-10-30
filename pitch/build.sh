#!/usr/bin/env bash
# pitch/*.md -> pitch/Наследие-питч.pdf (pandoc -> html -> headless chrome)
set -euo pipefail
cd "$(dirname "$0")"
out=build.html
{
  echo '<!doctype html><meta charset="utf-8"><link rel="stylesheet" href="style.css">'
  echo '<div class="cover"><h1>Наследие. Сквозь время</h1><p>Материалы для защиты проекта</p><p>Команда «Наследники», Владивосток</p></div>'
  for f in 0[0-9]-*.md; do
    echo '<section class="doc">'
    pandoc -f markdown -t html "$f"
    echo '</section>'
  done
} > "$out"
google-chrome --headless --disable-gpu --no-pdf-header-footer --print-to-pdf=Наследие-питч.pdf "file://$PWD/$out" 2>/dev/null
rm "$out"
ls -l Наследие-питч.pdf
