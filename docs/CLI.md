# SketchPen CLI Reference (`SketchPen.exe`)

`SketchPen.exe` is the command-line tool for rendering `.sp` icon scripts. It is
built on [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine)
and the .NET Generic Host (dependency injection), and is the primary way to
produce icons — it exposes every export mode (PNG, SVG, ZIP batches, the
CSS-variable HTML page), via the [`compose`](#the-compose-subcommand)
subcommand, in addition to its original render syntax. It has no interactive
UI of its own — use the [VS Code extension](../README.md#vs-code-extension)
(which shells out to this CLI) if you want a code editor with live preview.

**The original invocation syntax (documented in [Usage](#usage) below) is
unchanged and fully supported** — it is the root command's own behavior, not a
separate legacy mode; existing scripts keep working without modification.

Source: [`src/SketchPen/Program.cs`](../src/SketchPen/Program.cs),
[`src/SketchPen/Commands/RenderCommandHandler.cs`](../src/SketchPen/Commands/RenderCommandHandler.cs),
[`src/SketchPen/Commands/ComposeCommandHandler.cs`](../src/SketchPen/Commands/ComposeCommandHandler.cs),
[`src/SketchPen/Plotter.cs`](../src/SketchPen/Plotter.cs).
For the scripting language itself (`.sp`/`.spt`/`.globals`), see
[docs/SYNTAX.md](SYNTAX.md).

## Contents

- [Installation](#installation)
- [Build](#build)
- [Usage](#usage)
- [Arguments](#arguments)
- [What gets rendered](#what-gets-rendered)
- [Output naming and location](#output-naming-and-location)
- [Vector (SVG) output](#vector-svg-output)
- [Styling (custom globals)](#styling-custom-globals)
- [Examples](#examples)
- [The `compose` subcommand](#the-compose-subcommand)
- [Machine-readable output (`--output json`)](#machine-readable-output---output-json)
- [The `language-info` subcommand](#the-language-info-subcommand)
- [Convenience scripts (`plot.bat` / `plot.sh`)](#convenience-scripts-plotbat--plotsh)
- [Exit codes and error output](#exit-codes-and-error-output)
- [Known limitations](#known-limitations)

## Installation

Published as the **`SketchPen.Cli`** .NET global tool (portable/framework-
dependent — one NuGet package, works on Windows, Linux, and macOS wherever the
.NET SDK/runtime is installed):

```bash
dotnet tool install -g SketchPen.Cli    # first install
dotnet tool update -g SketchPen.Cli     # later, to upgrade
```

This installs the command as **`sketchpen`** (lowercase). Every example on
this page uses `SketchPen.exe` (the source-build name on Windows) — after a
global-tool install, swap that for `sketchpen`; behavior is identical either
way, only the program name differs (`dotnet SketchPen.dll` is the equivalent
when running a source build on Linux/macOS instead of `sketchpen`).

New versions are published automatically by
[`.github/workflows/release.yml`](../.github/workflows/release.yml) on every
`vX.Y.Z` git tag push — see [docs/RELEASING.md](RELEASING.md) for how to test
the packaged tool locally and how to cut a release.

## Build

```bash
dotnet build src/SketchPen/SketchPen.csproj -c Release
```

The resulting executable/DLL is written to
`src/SketchPen/bin/Release/net10.0/SketchPen.exe` (Windows) resp.
`SketchPen.dll`, run via `dotnet SketchPen.dll ...` on other platforms. For a
self-contained single-file executable, use `dotnet publish` with the usual
`--self-contained` / `-r <runtime-id>` options. To build the installable
global-tool package yourself instead of downloading it, see
[Installation](#installation) and `dotnet pack src/SketchPen/SketchPen.csproj -c Release`.

## Usage

```bash
SketchPen.exe <path> [-outfolder <folder>] [-custom_globals <stylename>] [-format png|svg] [--output text|json]
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
| `--output text\|json` | No | Console output mode — see [Machine-readable output](#machine-readable-output---output-json). Default: `text` (today's exact console output). |

Every option accepts both its original single-dash spelling (`-outfolder`,
`-custom_globals`, `-format`) and a newer double-dash kebab-case alias
(`--outfolder`, `--custom-globals`, `--format`) — both work identically, use
whichever you prefer. Options take exactly one value and can be given in any
order. There is currently no option to change the rendered PNG sizes/resolutions
on the root command — see [Known limitations](#known-limitations) and
[The `compose` subcommand](#the-compose-subcommand) for a way to do that today.

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
per-size/per-style subfolder structure (unlike the `png-zip` composer's ZIP
layout, see [the `compose` subcommand](#the-compose-subcommand)).

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
`_<stylename>.globals` are combined. To render several styles with the root
command, invoke the CLI once per style with a different output folder or
filename convention, e.g.:

```bash
SketchPen.exe plot/basic -outfolder plot/basic-img
SketchPen.exe plot/basic -outfolder plot/basic-img-dark -custom_globals bg-dark
SketchPen.exe plot/basic -outfolder plot/basic-img-e    -custom_globals e
```

(The [`compose`](#the-compose-subcommand) subcommand, in contrast, accepts a
comma-separated `--styles` list and bundles all of them into a single ZIP in
one run — see the examples there.)

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

## The `compose` subcommand

```bash
SketchPen.exe compose <path> --composer <id> [--sizes 16,32,64] [--styles ,bg-dark] [--resolutions 96,144] [--out <file>]
```

Invokes any registered `IComposerService` directly — the same abstraction the
VS Code extension's `Export Package…` command uses too — instead of the root
command's fixed PNG/SVG behavior. Useful whenever you need a ZIP batch, a web
sprite, or the CSS-variable HTML page from the command line, or want a
different SVG size than the root command's fixed reference size (see
[Vector (SVG) output](#vector-svg-output)).

> **Common mistake:** `<path>` must come right **after** `compose`, not before
> it — `SketchPen.exe compose plot/webgis --composer svg-zip` is correct,
> `SketchPen.exe plot/webgis compose --composer svg-zip` is **not**. The root
> command (see [Usage](#usage)) also accepts a `<path>` argument; if it appears
> before `compose`, the root command consumes it and `compose` is left without
> one, failing with `Missing required <path> argument`. Everything *after* the
> `compose` keyword — the path and all `--options` — can be given in any order.
> Run `SketchPen.exe compose --help` for the full option list and worked
> examples (also reproduced below).

| Option | Required | Description |
|---|---|---|
| `<path>` (positional) | Yes | Same file-or-directory semantics as the root command's `<path>`. |
| `--composer <id>` | Yes | Which composer to invoke — see the table below. |
| `--sizes <csv>` | No | Comma-separated sizes, e.g. `16,32,64`. Composers that export a single icon (`png`, `svg`) require exactly one value; batch composers pick a sensible default or reference size if omitted (see each composer's behavior). |
| `--styles <csv>` | No | Comma-separated style names; an empty entry means the default style. Omitted = default style only, matching `--styles ""`. Multiple styles only make sense with the batch (`*-zip`) composers. |
| `--resolutions <csv>` | No | Comma-separated DPI values, e.g. `96,144,192`. Only meaningful for `png-zip`/`web-sprite-zip`; ignored by the SVG composers (vector output is resolution-independent). |
| `--out <file>` | No | Output file path. Default: `<name>.<extension>` in the current directory, where `<name>` is the last path segment of `<path>` and `<extension>` is the composer's own file extension. |

Composer ids (see [README.md](../README.md#export-composers) for the full
description of each, including the CSS-variable mechanism and its
inline-SVG requirement — the id is just a short, stable name for the
underlying `IComposerService`, used in place of a full CLR type name):

| `--composer` id | Name | Output |
|---|---|---|
| `png` | Image | A single PNG. Requires exactly one size, one style, one `.sp` file. |
| `png-zip` | Images | ZIP with all icons of a set, split into `style/size/resolution/`. |
| `web-sprite-zip` | Web Sprites | ZIP with CSS sprite PNGs, generated CSS, and a demo HTML page. |
| `svg` | Vector Image | A single SVG. Requires exactly one size, one style, one `.sp` file. |
| `svg-zip` | Images (SVG) | ZIP with all icons of a set as individual SVGs, split into `style/`. |
| `svg-vars-zip` | Images (CSS Variables) | ZIP with one self-contained, themeable HTML page per icon — see the README for the CSS-variable mechanism and its inline-SVG requirement. |

Examples:

```bash
# One SVG at a custom reference size (the root command's -format svg always uses 128)
SketchPen.exe compose plot/basic/disk.sp --composer svg --sizes 64 --out disk.svg

# Whole set, two styles, as individual SVGs in a zip
SketchPen.exe compose plot/webgis --composer svg-zip --styles ",bg-dark" --out webgis-svg.zip

# Whole set as themeable CSS-variable HTML pages
SketchPen.exe compose plot/basic --composer svg-vars-zip --out basic-themeable.zip

# CSS sprite sheet + generated CSS + a demo HTML page, all in one ZIP
SketchPen.exe compose plot/basic --composer web-sprite-zip --sizes 16,32,64 --out basic-sprites.zip
```

An unknown `--composer` id fails with a clear error listing the valid ids
(exit code `1`), consistent with [Exit codes and error output](#exit-codes-and-error-output).

## Machine-readable output (`--output json`)

Both the root command and `compose` accept `--output json` to emit exactly one
JSON document to stdout instead of the human-readable progress text — intended
for tooling (e.g. an editor extension) that invokes `SketchPen.exe` and wants a
single structured result rather than parsing text. Exit codes (`0`/`1`) are
identical between `text` and `json` modes.

Shape:

```jsonc
// success
{"success": true, "filesWritten": ["disk_16@1.png", "..."], "error": null, "message": null}

// failure (compile-time syntax error in a .sp/.spt/.globals file)
{"success": false, "filesWritten": [], "error": {"type": "syntax", "message": "...", "codeFile": "...", "statement": "..."}, "message": null}

// failure (any other error, e.g. bad path, unknown composer id)
{"success": false, "filesWritten": [], "error": {"type": "generic", "message": "...", "codeFile": null, "statement": null}, "message": null}

// no <path> given
{"success": true, "filesWritten": [], "error": null, "message": "Usage: SketchPen.exe path [options]"}
```

## The `language-info` subcommand

```bash
SketchPen.exe language-info [path]
```

A tooling-only command that always prints exactly one JSON document — there's
no `--output`/text mode, since this isn't meant for direct human reading. It
exposes the reflection-driven completion grammar (methods/keywords per file
type) and, given a project path, that project's compiled `@@variable` names
— this is what powers the [VS Code extension](../vscode-extension)'s
completion, without the extension needing to duplicate any compiler logic:

```jsonc
{
  "success": true,
  "commands": {
    "code":     { "pen": [{"method":"color","suggestion":"...","snippet":"..."}, ...], "line": [...], ... },
    "template": { /* same shape, only keywords valid in .spt files */ },
    "globals":  { /* same shape, only keywords valid in .globals files: pen/brush/gradientbrush/globals */ }
  },
  "globalVariables": null  // or e.g. ["penColor","brushColor",...] when [path] was given
}
```

- With no `[path]`: `globalVariables` is `null`, `commands` is always present
  (it's static — the same grammar regardless of any project).
- With `[path]` (a directory, or a `.sp` file inside one): additionally
  compiles and executes that directory's `_.globals`, returning its
  `globals.set`/`tryset` variable names as `globalVariables`. A missing or
  broken `_.globals` yields `globalVariables: []`, **not** a command failure —
  `_.globals` is optional everywhere else in the language too, so this command
  never fails just because a project doesn't have one (yet).
- The only failure mode is a `[path]` that doesn't exist: `{"success": false,
  "commands": null, "globalVariables": null, "error": "Can't find part of the
  path '...'"}`, exit code `1`.

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

- **The root command's rendered PNG sizes/resolutions are fixed** (`16, 26, 32,
  64, 128` × `@1/@2/@3`), and its SVG output uses one fixed reference size
  (`128`) — this preserves the original tool's exact, unconfigurable behavior.
  Use [`compose`](#the-compose-subcommand) (`--composer png-zip`/`svg`/`svg-zip`
  with `--sizes`/`--resolutions`) for configurable sizes/resolutions/batches.
- **The root command only supports one style per invocation** — see
  [Styling](#styling-custom-globals). `compose --styles a,b,c` supports multiple
  styles in one call for the batch composers.
- No `DrawImage`/embedded-raster support in SVG output, no text-to-path
  outlining — see [Vector (SVG) output](#vector-svg-output). Applies to both the
  root command and every SVG-based composer.
