# Changelog

All notable changes to the SketchPen VS Code extension are documented here.

## 0.1.0

Initial release. Requires the [`sketchpen` CLI](../docs/CLI.md#installation)
(`dotnet tool install -g SketchPen.Cli`) — this extension shells out to it for
every rendering/compiling operation rather than reimplementing the language.

- **`SketchPen: Init`** — scaffolds a new icon-set folder: `_.globals`, a
  `templates/` folder with an example `.spt`, and a starter `icon.sp`. Never
  overwrites existing files.
- **Completion** — keyword/method completion (with parameter snippets) and
  `@@variable` completion, filtered per file type (`.sp`/`.spt`/`.globals`).
- **Hover** — hovering `@@name` shows its declared value from the folder's
  `_.globals`.
- **Live preview** — renders the active `.sp` file on save into a preview
  panel; also re-renders automatically when a `#include`d `.spt` file it
  depends on is saved.
- **Diagnostics** — compile errors surface as editor warnings, including
  inside `#include`d `.spt` files. Line-accurate, not column-accurate.
- **Syntax highlighting** for keywords, strings, comments, numbers, and
  `@@variable` references.
- **CodeLens** — inline `Preview` / `Export SVG` / `Export PNG` links above
  every `.sp` file.
- **Export commands** — Command Palette, editor title bar, and Explorer
  right-click menu (`SketchPen` submenu), wrapping every `compose` composer:
  single SVG/PNG, batch ZIPs, web sprites, themeable CSS-variable HTML.
  `Export as PNG` can optionally produce multiple `@1x`/`@2x`/`@3x`-style
  resolutions in one go; `Export Package…` exposes DPI-based `--resolutions`
  for `png-zip`/`web-sprite-zip`.
- **Getting Started walkthrough** — install the CLI, scaffold a project,
  preview, and export, from VS Code's own Get Started page.
- CLI-not-found detection with an install prompt (`SketchPen: Install CLI`).
