# SketchPen CLI Reference (`SketchPen.exe`)

`SketchPen.exe` is the command-line tool that batch-renders `.sp` icon scripts to
PNG files. It is the tool typically used to (re-)generate a whole icon set as part
of a build/CI step. It has no interactive UI — use the
[web application](../README.md#web-application-sketchpencode) if you want a code
editor with live preview.

Source: [`src/SketchPen/Program.cs`](../src/SketchPen/Program.cs),
[`src/SketchPen/Plotter.cs`](../src/SketchPen/Plotter.cs).
For the scripting language itself (`.sp`/`.spt`/`.globals`), see
[docs/SYNTAX.md](SYNTAX.md).

## Contents

- [Build](#build)
- [Usage](#usage)
- [Arguments](#arguments)
- [What gets rendered](#what-gets-rendered)
- [Output naming and location](#output-naming-and-location)
- [Vector (SVG) output](#vector-svg-output)
- [Styling (custom globals)](#styling-custom-globals)
- [Examples](#examples)
- [Convenience scripts (`plot.bat` / `plot.sh`)](#convenience-scripts-plotbat--plotsh)
- [Exit codes and error output](#exit-codes-and-error-output)
- [Known limitations](#known-limitations)

## Build

```bash
dotnet build src/SketchPen/SketchPen.csproj -c Release
```

The resulting executable/DLL is written to
`src/SketchPen/bin/Release/net10.0/SketchPen.exe` (Windows) resp.
`SketchPen.dll`, run via `dotnet SketchPen.dll ...` on other platforms. For a
self-contained single-file executable, use `dotnet publish` with the usual
`--self-contained` / `-r <runtime-id>` options.

## Usage

```bash
SketchPen.exe <path> [-outfolder <folder>] [-custom_globals <stylename>] [-format png|svg]
```

Running the tool with no arguments prints a short usage line and exits
successfully (exit code `0`):

```
Usage: SketchPen.exe path [options]
```

## Arguments

| Argument | Required | Description |
|---|---|---|
| `<path>` (first, positional) | Yes | Either a single `.sp` file, or a directory containing `.sp` files. See [What gets rendered](#what-gets-rendered). |
| `-outfolder <folder>` | No | Directory the generated PNGs are written to. Created automatically if it doesn't exist. Default: current working directory. |
| `-custom_globals <stylename>` | No | Name of a style, without leading underscore and without the `.globals` extension. Loads `_<stylename>.globals` from the same directory as the input file(s) in addition to the default `_.globals`. See [Styling](#styling-custom-globals) and [docs/SYNTAX.md](SYNTAX.md#globals-files-and-styling). Default: no custom style (only `_.globals`, if present). |
| `-format png\|svg` | No | Output format. `png` (default) renders the fixed raster size/resolution matrix described below. `svg` renders one resolution-independent vector file per icon instead — see [Vector (SVG) output](#vector-svg-output). Default: `png`. |

Options take exactly one value and can be given in any order. There is
currently no option to change the rendered PNG sizes/resolutions — see
[Known limitations](#known-limitations).

## What gets rendered

- If `<path>` points to a **single file**, that file is rendered.
- If `<path>` points to a **directory**, every `*.sp` file directly inside it is
  rendered (non-recursive — files in subfolders such as `templates/` are not
  picked up directly, they are only pulled in via `#include`). `.spt` and
  `.globals` files are never rendered on their own.
- If `<path>` is neither an existing file nor an existing directory, the tool
  exits with an error (`Can't find part of the path '<path>'`).

## Output naming and location

For every rendered `.sp` file, `-format png` (the default) always generates **5
sizes × 3 resolutions = 15 PNGs**, with fixed sizes `16, 26, 32, 64, 128`
(logical/CSS pixels) and resolution multipliers `@1`, `@2`, `@3` (i.e. actual
bitmap pixels = `size × ratio`):

```
<iconname>_<size>@<ratio>.png
```

For example, `admin.sp` produces:

```
admin_16@1.png    (16×16 px)
admin_16@2.png    (32×32 px)
admin_16@3.png    (48×48 px)
admin_26@1.png    (26×26 px)
admin_26@2.png    (52×52 px)
admin_26@3.png    (78×78 px)
admin_32@1.png … admin_128@3.png (384×384 px)
```

This is exactly the naming scheme used for the pre-rendered
[`plot/basic-img`](../plot/basic-img) and [`plot/webgis-img`](../plot/webgis-img)
folders, and matches common `@1x`/`@2x`/`@3x` asset-catalog conventions (iOS,
Android, Electron, …). `-format svg` produces one `<iconname>.svg` per icon
instead — see [Vector (SVG) output](#vector-svg-output).

Files are written to `<outfolder>/` (or the current directory if `-outfolder`
is omitted); the folder is created if it doesn't exist yet. All sizes/styles of
one CLI run are written **flat into the same folder** — there is no
per-size/per-style subfolder structure (unlike the web app's "Images" ZIP
composer).

## Vector (SVG) output

```bash
SketchPen.exe plot/basic/disk.sp -outfolder out -format svg
```

writes a single `disk.svg` to `out/` — **one file per icon**, not one per
size/resolution: SVG is resolution-independent, so multiplying by the `@1/@2/@3`
raster-DPI axis (or by the 5 standard sizes) would just produce near-duplicate
files. The icon is rendered once, internally at a fixed reference width of
**128** logical units — this only affects the pen-width min/max clamping (see
[docs/SYNTAX.md](SYNTAX.md#coordinate-system)), i.e. how thick strokes look
relative to the shape; it does **not** limit how large the resulting SVG can be
displayed, since vector output scales losslessly to any size.

Notes:
- `gradientbrush` is exported as a native SVG `<linearGradient>`.
- `text.draw` is exported as a native SVG `<text font-family="...">` element —
  rendering depends on the viewer having that font installed, so appearance may
  differ slightly from the PNG rendition (which bakes in whatever font Skia
  resolved at generation time). There is no text-to-path outlining.
- Combining `-format svg` with `-custom_globals <stylename>` works exactly like
  the PNG path (see [Styling](#styling-custom-globals)) — one style per run.

## Styling (custom globals)

`-custom_globals <stylename>` selects **one** style for the whole run — see
[docs/SYNTAX.md](SYNTAX.md#globals-files-and-styling) for how `_.globals` and
`_<stylename>.globals` are combined. To render several styles, invoke the CLI
once per style with a different output folder or filename convention, e.g.:

```bash
SketchPen.exe plot/basic -outfolder plot/basic-img
SketchPen.exe plot/basic -outfolder plot/basic-img-dark -custom_globals bg-dark
SketchPen.exe plot/basic -outfolder plot/basic-img-e    -custom_globals e
```

(The web app's `Package` endpoint, in contrast, accepts a comma-separated list of
styles and bundles all of them into a single ZIP in one request — see the
[README](../README.md#web-application-sketchpencode).)

## Examples

Render every icon in `plot/basic` into `plot/basic-img` with the default style:

```bash
SketchPen.exe plot/basic -outfolder plot/basic-img
```

Same, but with the `bg-dark` color style (`plot/basic/_bg-dark.globals`):

```bash
SketchPen.exe plot/basic -outfolder plot/basic-img -custom_globals bg-dark
```

Render the `webgis` set:

```bash
SketchPen.exe plot/webgis -outfolder plot/webgis-img
```

## Convenience scripts (`plot.bat` / `plot.sh`)

[`plot/plot.bat`](../plot/plot.bat) (Windows) and [`plot/plot.sh`](../plot/plot.sh)
(Linux/macOS, via `dotnet SketchPen.dll`) are thin wrappers that call the CLI with
`%1`/`$1` as the set name and `%2`/`$2` as an optional style name:

```bash
# Windows
plot.bat basic bg-dark

# Linux/macOS
./plot.sh basic bg-dark
```

which is equivalent to `SketchPen.exe plot/basic -outfolder plot/basic-img
-custom_globals bg-dark`. Both scripts currently hard-code an absolute path to
the built executable/DLL (a leftover `net6.0` path in `plot.sh`) — adjust it to
match your local build output (`net10.0`) and machine before using them as-is.

## Exit codes and error output

| Exit code | Meaning |
|---|---|
| `0` | Success, or no arguments given (usage line printed). |
| `1` | A syntax error in a `.sp`/`.spt`/`.globals` file, or any other exception (missing path, unknown command, invalid parameters, I/O error, …). |

On a script syntax error, the tool prints the offending file and the exact
statement that failed, e.g.:

```
plot/basic/admin.sp
ERROR: Unknown method: fil
>>
>> circle.fil(50,50, 0,-15);
>>
```

For any other exception, the message is printed (`Exception: <message>`); in a
`Debug` build the stack trace is also printed.

## Known limitations

- **Rendered PNG sizes/resolutions are fixed** (`16, 26, 32, 64, 128` × `@1/@2/@3`)
  and cannot currently be changed via a CLI option, unlike the web app's
  `Package` endpoint (`sizes`/`resolutions` query parameters).
- **Only one style per invocation** — see [Styling](#styling-custom-globals).
- **SVG output is one fixed reference size per icon** (128) — no CLI option to
  pick a different reference size (the web app's "Vector Image"/"Images (SVG)"
  composers do accept a `sizes` parameter for this). No `DrawImage`/embedded-
  raster support, no text-to-path outlining — see
  [Vector (SVG) output](#vector-svg-output).
