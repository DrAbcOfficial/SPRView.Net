# License scope

This file explains **what** the license in [LICENSE](LICENSE) covers. It is not
itself a license, and it does not modify the license terms.

## This repository's own code

Everything written for this project is licensed under the **GNU Lesser General
Public License, version 3 or later** (`LGPL-3.0-or-later`).

That covers the sources under `src/`, `platform/` and `tools/`: the Core
library, the Avalonia viewer, the CLI, the thumbnailer, the Windows shell
extension, the macOS Quick Look extension and the Linux XDG integration.

```
Copyright (C) 2024-2026 Dr.Abc

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along
with this program. If not, see <https://www.gnu.org/licenses/>.
```

## What it does not cover

The LGPL applies to this repository's own code only. It does **not** relicense
anything the project depends on or ships alongside, and it does not change the
terms those components are distributed under.

| Component | License | Role |
|--|--|--|
| [Avalonia UI](https://avaloniaui.net/) | MIT | UI framework |
| [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) | Six Labors Split License | Image decoding, encoding and quantization |
| [CliFx](https://github.com/Tyrrrz/CliFx) | MIT | Command line parsing in `SPRView.Net.CLI` |
| .NET / NativeAOT runtime | MIT | Base class library and the AOT toolchain |
| Windows SDK, macOS SDK, X11/GTK headers | respective platform terms | Platform integration |

The **Six Labors Split License** is worth calling out because it is not a plain
permissive license: it grants Apache-2.0 terms when the consumer is open source,
a transitive dependency, a small for-profit, or a non-profit, and requires a
commercial license otherwise. See the license text shipped in the
`SixLabors.ImageSharp` package for the exact conditions. If you intend to reuse
this project's code in a closed source product, check that clause yourself; the
LGPL above does not settle it for you.

## The LGPL incorporates the GPL

The Lesser GPL is a set of additional permissions on top of the ordinary GPL,
not a standalone document. [LICENSE](LICENSE) is therefore the LGPL-3.0 text and
[LICENSE.GPL-3.0.txt](LICENSE.GPL-3.0.txt) is the GPL-3.0 text it incorporates.
Both are required to read the terms in full, and both are the unmodified
documents as published by the FSF.
