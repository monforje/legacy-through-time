#!/usr/bin/env python3
"""Compile Assets/Scripts/Prototype against the Unity DLLs without opening the editor.

The generated LegacyThroughTime.Prototype.csproj lists source files explicitly and goes stale between editor
sessions, so this takes its references and defines, swaps the file list for a glob and builds into a temp folder.
Needs `dotnet` and a built Library/ (the editor has been opened once). Prints the compiler errors, exit code 1 on any.

    python3 tools/compile_check.py
"""
import pathlib
import re
import subprocess
import sys
import tempfile

ROOT = pathlib.Path(__file__).resolve().parent.parent
src = (ROOT / 'LegacyThroughTime.Prototype.csproj').read_text()
src = re.sub(r'\s*<(Compile|None) Include="[^"]*"\s*/>', '', src)
src = re.sub(r'<ProjectReference Include="([^"/][^"]*)"', lambda m: f'<ProjectReference Include="{ROOT}/{m.group(1)}"', src)
src = re.sub(r'(<HintPath>)(?!/)([^<]*)', lambda m: f'{m.group(1)}{ROOT}/{m.group(2)}', src)
src = src.replace('</Project>', f'  <ItemGroup><Compile Include="{ROOT}/Assets/Scripts/Prototype/**/*.cs" /></ItemGroup>\n</Project>')
with tempfile.TemporaryDirectory() as tmp:
    proj = pathlib.Path(tmp) / 'check.csproj'
    proj.write_text(src)
    run = subprocess.run(['dotnet', 'build', str(proj), '-nologo', '-v', 'q', '-o', str(pathlib.Path(tmp) / 'out')], capture_output=True, text=True)
errors = sorted({line.strip() for line in (run.stdout + run.stderr).splitlines() if ': error ' in line})
print('\n'.join(e.rsplit(' [', 1)[0] for e in errors) if errors else 'compile ok')
sys.exit(1 if errors else 0)
