# SketchPen

SketchPen is a small toolkit for **defining icons once as a script** and generating
any number of PNG or SVG variants from them – in different sizes, resolutions
(`@1x`/`@2x`/`@3x`), and color styles ("themes"). Instead of maintaining hand-drawn
SVGs or pixel-perfect bitmaps per icon, you describe shape, pen, and fill as a
simple text file (`.sp`), which is rendered at run time.

```csharp
// database.sp
path.begin();
path.addarc(0,180, 80,20, 0,-35);
path.addlines(-40,-35, -40,35);
path.addarc(180,-180, 80,20, 0,35);
path.close();

path.fill();
path.draw(@@outlinePenColor);

path.begin();
path.addarc(180,180, 80,20, 0,-35);
path.draw(@@outlinePenColor);

path.begin();
path.addarc(180,-180, 80,20, 0,-11.67);
path.draw();

path.begin();
path.addarc(180,-180, 80,20, 0,11.67);
path.draw();

path.begin();
path.addarc(0,180, 80,20, 0,-35);
path.addlines(-40,-35, -40,35);
path.addarc(180,-180, 80,20, 0,35);
path.close();

path.draw(@@outlinePenColor);
```

turns into e.g. `database_16@1.png`, `database_16@2.png`, …, `database_128@3.png` – in as
many color/style variants as you like, driven by `.globals` files.

➡️ The full language reference (syntax, coordinate system, all commands, variables,
`.globals` styling, `#include`) lives in **[docs/SYNTAX.md](docs/SYNTAX.md)**.

## Ways to create icons

There are two ways to work with SketchPen:

1. **VS Code extension** – edit `.sp`/`.spt`/`.globals` files with completion,
   live preview, syntax highlighting, inline diagnostics, and export commands,
   right in the editor. Not yet published to the VS Code Marketplace — install
   it from this repo, see [VS Code extension](#vs-code-extension) below.
2. **`SketchPen` (CLI)** – renders a single script or an entire directory to PNGs in
   several standard sizes and resolutions, or to one resolution-independent SVG per
   icon, suitable for scripting/CI; the `compose` subcommand additionally exposes
   every composer (batch PNG/SVG, web sprites, themeable CSS-variable HTML). The
   extension itself shells out to this CLI for every render.

There used to be a third option, a browser-based editor (`SketchPen.Code`,
an ASP.NET Core MVC app) — it has been replaced by the VS Code extension and
is no longer part of the build (its source is still in the repo under
`src/SketchPen.Code/`, unbuilt, for reference only).

Ready-made example icon sets live under [`plot/`](plot):

| Directory                        | Content                                                                                                                                  |
| -------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| [`plot/basic`](plot/basic)       | ~180 general UI icons (account, arrows, folder, trash can, …) incl. templates and color styles                                           |
| [`plot/webgis`](plot/webgis)     | GIS/map-specific icons (markers, measuring, construction tools, …)                                                                       |
| [`plot/gview`](plot/gview)       | Axis/coordinate system symbols                                                                                                           |
| [`plot/examples`](plot/examples) | Language syntax showcase — one heavily-commented `.sp` file per feature (existing and newly added), see [docs/SYNTAX.md](docs/SYNTAX.md) |
| `plot/*-img`                     | Pre-rendered PNG output of the respective sets (example output of the CLI tool)                                                          |

## Project structure

```
src/
  SketchPen.Parse         Lexer/tokenizer for the SketchPen language
  SketchPen.Plot           Compiler, commands (pen/brush/path/circle/...), abstractions
  SketchPen.Plot.Skia       Rendering backend based on SkiaSharp (cross-platform, the only backend)
  SketchPen.Compose         Export/packaging logic (single PNG/SVG, ZIP with sizes/styles, web sprite + CSS)
  SketchPen               Command-line tool (SketchPen.exe)
  SketchPen.Code           Former ASP.NET Core web app -- no longer built (excluded from
                           SketchPen.sln), kept in the repo for reference only
vscode-extension/          VS Code extension (TypeScript, not part of SketchPen.sln) -- see
                           "VS Code extension" below and vscode-extension/README.md
plot/                      Example icon sets (scripts, templates, styles, rendered PNGs)
docs/SYNTAX.md              Language reference for .sp / .spt / .globals
docs/CLI.md                 SketchPen.exe command-line reference
```

Processing pipeline: **script (`.sp`/`.spt`/`.globals`) → `PreComplier`** (resolve
includes, prepend globals, normalize syntax) **→ `Lexer`/`Compiler`** (tokens →
command list) **→ `IPlotContext` implementation** (raster `PlotContext` or vector
`SvgPlotContext`, both SkiaSharp-based) **→ PNG or SVG bytes**.

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/) (see `TargetFramework net10.0` in the
  CLI's `.csproj`) — needed to build/run the CLI, or just install it as a
  global tool (see below), no SDK required for that.
- Node.js 20+ and `npm` — only if you want to rebuild the VS Code extension
  from source; not needed to install the pre-built `.vsix`.

## Command-line tool (`SketchPen.exe`)

### Installation

Published as the **`SketchPen.Cli`** .NET global tool — requires the
[.NET SDK/runtime](https://dotnet.microsoft.com/) (any platform: Windows,
Linux, macOS):

```bash
dotnet tool install -g SketchPen.Cli
```

Installs the command as **`sketchpen`** (lowercase). Update later with:

```bash
dotnet tool update -g SketchPen.Cli
```

New versions are published automatically by
[`.github/workflows/release.yml`](.github/workflows/release.yml) whenever a
`vX.Y.Z` git tag is pushed — see [docs/RELEASING.md](docs/RELEASING.md) for how
to test the packaged tool locally and how to cut a release. Everything below
applies identically whether you installed the tool (`sketchpen ...`) or built
from source (`SketchPen.exe ...`/`dotnet SketchPen.dll ...`) — only the
program name differs.

### Build from source

```bash
dotnet build src/SketchPen/SketchPen.csproj -c Debug
```

Usage:

```bash
SketchPen.exe <directory> [-outfolder <path>] [-custom_globals <stylename>] [-format png|svg] [-sizes <csv>] [-resolutions <csv>]
```

```bash
SketchPen.exe plot/basic -outfolder plot/basic-img
SketchPen.exe plot/basic -outfolder plot/basic-img -custom_globals bg-dark
SketchPen.exe plot/basic -outfolder plot/basic-svg -format svg
SketchPen.exe plot/basic -outfolder out -sizes 32,64 -resolutions 1,2
```

By default, renders every `*.sp` file in `<directory>` to PNGs in the default
sizes (`16, 26, 32, 64, 128` px) and resolutions (`@1`/`@2`/`@3`), named
`<iconname>_<size>@<resolution>.png` — exactly the scheme that produces
[`plot/basic-img`](plot/basic-img) from [`plot/basic`](plot/basic). Both lists
are overridable via `-sizes`/`-resolutions` (PNG only). With `-format svg`, it
instead renders one resolution-independent `<iconname>.svg` per icon. This
makes it straightforward to script icon generation or wire it into CI.
**This original syntax is fully preserved** — it's the built-in behavior of the
tool's root command, not a deprecated compatibility shim.

Built on `System.CommandLine` + the .NET Generic Host (dependency injection),
the CLI additionally exposes every export mode directly via a `compose`
subcommand — every registered `IComposerService` (single/batch PNG,
single/batch SVG, web sprites, themeable CSS-variable HTML), selected by a
short id:

```bash
SketchPen.exe compose plot/basic/disk.sp --composer svg --sizes 64 --out disk.svg
SketchPen.exe compose plot/webgis --composer svg-vars-zip --styles ",bg-dark" --out webgis-themeable.zip
```

Both the root command and `compose` accept `--output json` to emit one
structured result to stdout instead of human-readable text, for tooling that
invokes `SketchPen.exe` programmatically.

➡️ Full parameter reference, the `compose` subcommand, composer ids, JSON output
shape, output naming, styling, exit codes, and known limitations:
**[docs/CLI.md](docs/CLI.md)**.

## Export composers

Every export mode (single PNG/SVG, batch ZIPs, web sprite, themeable HTML) is
an `IComposerService` implementation in `SketchPen.Compose`, selected by id
via the CLI's [`compose`](#command-line-tool-sketchpenexe) subcommand or the
VS Code extension's `Export Package…` command:

- **`png`** (Image) – a single PNG (exactly one size, one style).
- **`png-zip`** (Images) – ZIP with all icons of a set, split into `style/size/resolution/`.
- **`web-sprite-zip`** (Web Sprites) – ZIP with CSS sprite PNGs per style/size, generated CSS
  (incl. `@media` rules for `@2x`/`@3x`), and a `sketchpen.html` preview page.
- **`svg`** (Vector Image) – a single SVG (exactly one icon, one style).
- **`svg-zip`** (Images (SVG)) – ZIP with all icons of a set as individual SVGs, split into
  `style/` (no size/resolution split — SVG is resolution-independent).
- **`svg-vars-zip`** (Images (CSS Variables)) – ZIP with all icons of a set, split into `style/`,
  each as a self-contained HTML page: the icon as an inline SVG whose colors are
  wired to CSS custom properties (`--sketchpen-<globalsName>`, e.g.
  `--sketchpen-penColor`), a matching `:root` block with the default values, a
  copy-paste-ready escaped snippet, and one color picker per variable to try
  live restyling. The colors substituted are exactly the ones sourced from
  `.globals` variables (see
  [docs/SYNTAX.md](docs/SYNTAX.md#globals-files-and-styling)) — colors set
  directly inline in a `.sp` script stay literal. **The SVG must stay inline in
  the consuming page's DOM** for the CSS variables to resolve — loading it via
  `<img src="icon.svg">` or a CSS `background-image` reference puts it in an
  isolated document that can't see the page's `--sketchpen-*` variables.

## VS Code extension

Edit `.sp`/`.spt`/`.globals` files with completion, live preview, syntax
highlighting, inline diagnostics, and export commands, right in the editor.
It is a thin client that shells out to the `sketchpen` CLI for every
render/compile — no logic is duplicated in TypeScript.

**Not yet published to the VS Code Marketplace** — build and install the
`.vsix` from this repo (it is not committed/pre-built, `git clone` alone is
not enough):

```bash
dotnet tool install -g SketchPen.Cli    # the extension needs this on PATH
cd vscode-extension
npm install && npm run compile && npx @vscode/vsce package
code --install-extension sketchpen-0.1.0.vsix
```

Full feature list, known limitations, and settings:
**[vscode-extension/README.md](vscode-extension/README.md)**.

## Further reading

- **Language reference**: [docs/SYNTAX.md](docs/SYNTAX.md) – coordinate system,
  all commands (`pen`, `brush`, `gradientbrush`, `line`, `rect`, `circle`, `path`,
  `text`, `transform`, `globals`), variables (`@@name`), arithmetic expressions,
  `.globals` styling, `#include`, `repeat`.
- **CLI reference**: [docs/CLI.md](docs/CLI.md) – all `SketchPen.exe` parameters,
  output naming, styling, exit codes, known limitations.
- **VS Code extension**: [vscode-extension/README.md](vscode-extension/README.md)
  – features, settings, known limitations.
- **Releasing**: [docs/RELEASING.md](docs/RELEASING.md) – how to test the
  packaged CLI locally and how to cut a release.
- **Syntax showcase**: [`plot/examples`](plot/examples) – one commented `.sp`
  file per language feature, the fastest way to learn the syntax by reading.
- **Real-world examples**: [`plot/basic`](plot/basic), [`plot/webgis`](plot/webgis),
  [`plot/gview`](plot/gview) – actual icon sets to read.
