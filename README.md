# SketchPen

SketchPen is a small toolkit for **defining icons once as a script** and generating
any number of PNG variants from them – in different sizes, resolutions
(`@1x`/`@2x`/`@3x`), and color styles ("themes"). Instead of maintaining SVGs or
pixel-perfect bitmaps per icon, you describe shape, pen, and fill as a simple text
file (`.sp`), which is rendered at run time.

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

turns into e.g. `csharp_16@1.png`, `csharp_16@2.png`, …, `csharp_128@3.png` – in as
many color/style variants as you like, driven by `.globals` files.

➡️ The full language reference (syntax, coordinate system, all commands, variables,
`.globals` styling, `#include`) lives in **[docs/SYNTAX.md](docs/SYNTAX.md)**.

## Ways to create icons

There are three ways to work with SketchPen:

1. **Write script files by hand** (`.sp` / `.spt` / `.globals`) and render them with
   the [command-line tool](#command-line-tool-sketchpenexe) or the
   [web application](#web-application-sketchpencode).
2. **`SketchPen` (CLI)** – renders a single script or an entire directory to PNGs in
   several standard sizes and resolutions, suitable for scripting/CI.
3. **`SketchPen.Code` (web app)** – browser-based editor with a file/style tree, live
   preview, and export as a ZIP package (individual images or web sprite + CSS).

Ready-made example icon sets live under [`plot/`](plot):

| Directory | Content |
|---|---|
| [`plot/basic`](plot/basic) | ~180 general UI icons (account, arrows, folder, trash can, …) incl. templates and color styles |
| [`plot/webgis`](plot/webgis) | GIS/map-specific icons (markers, measuring, construction tools, …) |
| [`plot/gview`](plot/gview) | Axis/coordinate system symbols |
| `plot/*-img` | Pre-rendered PNG output of the respective sets (example output of the CLI tool) |

## Project structure

```
src/
  SketchPen.Parse         Lexer/tokenizer for the SketchPen language
  SketchPen.Plot           Compiler, commands (pen/brush/path/circle/...), abstractions
  SketchPen.Plot.Drawing    Rendering backend based on System.Drawing (GDI+)
  SketchPen.Plot.Skia       Rendering backend based on SkiaSharp (cross-platform, used by CLI & web app)
  SketchPen.Compose         Export/packaging logic (single image, ZIP with sizes/styles, web sprite + CSS)
  SketchPen               Command-line tool (SketchPen.exe)
  SketchPen.Code           ASP.NET Core web application with a browser-based editor
plot/                      Example icon sets (scripts, templates, styles, rendered PNGs)
docs/SYNTAX.md              Language reference for .sp / .spt / .globals
docs/CLI.md                 SketchPen.exe command-line reference
```

Processing pipeline: **script (`.sp`/`.spt`/`.globals`) → `PreComplier`** (resolve
includes, prepend globals, normalize syntax) **→ `Lexer`/`Compiler`** (tokens →
command list) **→ `IPlotContext` implementation** (Drawing or Skia backend) **→ PNG
bytes**.

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/) (see `TargetFramework net10.0` in the
  `.csproj` files of the CLI/web app)

## Command-line tool (`SketchPen.exe`)

Build:

```bash
dotnet build src/SketchPen/SketchPen.csproj -c Debug
```

Usage:

```bash
SketchPen.exe <directory> [-outfolder <path>] [-custom_globals <stylename>]
```

```bash
SketchPen.exe plot/basic -outfolder plot/basic-img
SketchPen.exe plot/basic -outfolder plot/basic-img -custom_globals bg-dark
```

Renders every `*.sp` file in `<directory>` to PNGs in a fixed set of sizes
(`16, 26, 32, 64, 128` px) and resolutions (`@1`/`@2`/`@3`), named
`<iconname>_<size>@<resolution>.png` — exactly the scheme that produces
[`plot/basic-img`](plot/basic-img) from [`plot/basic`](plot/basic). This makes it
straightforward to script icon generation or wire it into CI.

➡️ Full parameter reference, output naming, styling, exit codes, and known
limitations: **[docs/CLI.md](docs/CLI.md)**.

## Web application (`SketchPen.Code`)

An ASP.NET Core MVC project with a browser-based code editor (Monaco) for
`.sp`/`.spt`/`.globals` files, a file/style tree, a live image preview per file, and
export as a downloadable package.

Build & run:

```bash
dotnet run --project src/SketchPen.Code/SketchPen.Code.csproj -- --rootPath="G:\github\jugstalt\SketchPen\plot"
```

- **`rootPath`** (configuration value resp. `--rootPath=...`) points to the
  directory containing the icon sets (e.g. the `plot` folder of this repo). Every
  subdirectory in it (e.g. `basic`, `webgis`) appears as its own set/`id` in the UI.
- Then in the browser: `https://localhost:<port>/Code/<id>` (e.g. `.../Code/basic`)
  opens the editor for that set.

Key features (see [`CodeController`](src/SketchPen.Code/Controllers/CodeController.cs)):

| Feature | Endpoint | Description |
|---|---|---|
| File tree | `GET /Code/GetFiles/{id}` | All `.sp`/`.spt` files of a set |
| Edit file | `GET/POST /Code/EditFile` | Read/save content, incl. editor autocomplete per file type |
| Create/delete file | `GET /Code/CreateFile/{id}`, `GET /Code/DeleteFile/{id}` | Create or remove a new `.sp`/`.globals` file |
| Style list | `GET /Code/GetGlobals/{id}` | All `_*.globals` styles of a set |
| Live preview | `GET /Code/Preview?route=...&globals=...&width=...&height=...` | Renders a single file live as a PNG |
| Package export | `GET /Code/Package?id=...&composer=...&styles=...&sizes=...&resolutions=...` | Builds a ZIP via the chosen composer |

Available export composers (`SketchPen.Compose`):

- **Image** – a single PNG (exactly one size, one style).
- **Images** – ZIP with all icons of a set, split into `style/size/resolution/`.
- **Web Sprites** – ZIP with CSS sprite PNGs per style/size, generated CSS
  (incl. `@media` rules for `@2x`/`@3x`), and a `sketchpen.html` preview page.

## Further reading

- **Language reference**: [docs/SYNTAX.md](docs/SYNTAX.md) – coordinate system,
  all commands (`pen`, `brush`, `gradientbrush`, `line`, `circle`, `path`, `text`,
  `transform`, `globals`), variables (`@@name`), `.globals` styling, `#include`.
- **CLI reference**: [docs/CLI.md](docs/CLI.md) – all `SketchPen.exe` parameters,
  output naming, styling, exit codes, known limitations.
- **Examples**: [`plot/basic`](plot/basic), [`plot/webgis`](plot/webgis),
  [`plot/gview`](plot/gview) – real-world `.sp`/`.spt`/`.globals` files to read.
