"""Reports localization keys that nothing references any more.

The two language files, the view model and the XAML/code that binds them have
to stay in step: a key left behind after a layout change is dead weight, and a
binding without a key renders blank. Run from the repository root:

    python scripts/check-lang-keys.py
"""

import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
UI = os.path.join(ROOT, "src", "SPRView.Net")


def read(path):
    with open(path, encoding="utf-8") as handle:
        return handle.read()


def main():
    english = json.loads(read(os.path.join(UI, "Assets", "Lang", "en.json")))
    chinese = json.loads(read(os.path.join(UI, "Assets", "Lang", "zh.json")))

    view_model = read(os.path.join(UI, "ViewModel", "LangViewModel.cs"))
    declared = set(re.findall(r"public string (\w+)\s*[{(]", view_model))

    problems = []

    if set(english) != set(chinese):
        problems.append("en.json and zh.json disagree: "
                        f"only-en={sorted(set(english) - set(chinese))} "
                        f"only-zh={sorted(set(chinese) - set(english))}")
    if set(english) != declared:
        problems.append("JSON and LangViewModel disagree: "
                        f"only-json={sorted(set(english) - declared)} "
                        f"only-vm={sorted(declared - set(english))}")

    # Everything outside the language files and the view model definition.
    referenced = []
    for pattern in ("**/*.axaml", "**/*.cs"):
        for path in glob.glob(os.path.join(UI, pattern), recursive=True):
            parts = path.replace(os.sep, "/").split("/")
            if "obj" in parts or "bin" in parts:
                continue
            if os.path.basename(path) == "LangViewModel.cs":
                continue
            referenced.append(read(path))
    blob = "\n".join(referenced)

    # Matched as whole identifiers, not as substrings: "TaskBar_File" occurs
    # inside "TaskBar_File_Create", so a plain containment test reports a dead
    # key as used and hides exactly the leftovers this is meant to catch.
    unused = sorted(key for key in english
                    if not re.search(rf"\b{re.escape(key)}\b", blob))
    if unused:
        problems.append(f"keys nothing references: {unused}")

    if problems:
        for line in problems:
            print(line)
        return 1

    print(f"localization keys ok ({len(english)} keys)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
