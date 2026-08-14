# Changelog

All notable changes to the SketchPen VS Code extension are documented here.

## Unreleased

- **`SketchPen: Preview Set`** — right-click a folder in the Explorer (or run
  from the Command Palette with an icon open) to preview every icon in it at
  once, as a grid: whole-set visual consistency at a glance instead of one
  icon in isolation. One CLI process renders the whole folder (the classic
  root command, not one `compose` call per icon), with its own style
  dropdown and a client-side name filter for large sets.
- **`AGENTS.md` scaffolding** — `SketchPen: Init` now also creates an
  `AGENTS.md` in the target folder: a self-contained syntax cheat sheet and
  workflow guide for AI coding agents (Claude Code, Copilot, Cursor, ...),
  including the crucial "render the icon, then look at it" loop and a couple
  of gotchas (the apostrophe-in-comment lexer limitation; numeric R,G,B
  colors being ambiguous with a short `circle` position). Lets you ask an
  agent to draw an icon by describing it in plain language and have it
  actually verify its own output.

- **`.sketchpen.json` styles-folder support** — globals files now live in a
  styles folder (a local `styles/` subfolder by default, or wherever an
  optional `.sketchpen.json`'s `stylesPath` points, possibly shared across
  several icon sets), renamed without a leading underscore (`_.globals` →
  `default.globals`, `_<name>.globals` → `<name>.globals`). `SketchPen: Init`
  now scaffolds `styles/default.globals`; Completion and Hover resolve the
  styles folder the same way the CLI does. Matches the `sketchpen` CLI's
  `-style` flag (renamed from `-custom_globals`) — update to a current CLI
  build together with this extension.
- **Bigger, scalable preview** — the rendered SVG now fills most of the
  panel (previously capped at its native pixel size); a coordinate-grid
  overlay (toggle in the panel toolbar) shows the `-50..+50` logical square
  from [docs/SYNTAX.md](../docs/SYNTAX.md#coordinate-system) for alignment,
  with a second **Coordinates** toggle for a small `x,y` label at every
  grid intersection; a style dropdown re-renders the preview with any named
  style from the icon's styles folder, not just the default.

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
