# SketchPen for VS Code

Edit `.sp`/`.spt`/`.globals` [SketchPen](https://github.com/jugstalt/SketchPen)
icon scripts with completion, live preview, syntax highlighting, and export —
right from the editor.

This extension is a thin client around the **`sketchpen` CLI**; it does not
render or compile anything itself. Install the CLI first:

```bash
dotnet tool install -g SketchPen.Cli
```

If `sketchpen` isn't found on your `PATH` when the extension activates,
you'll get a prompt with an install button. If it's installed somewhere not
on `PATH`, set `sketchpen.cliPath` in your VS Code settings.

## Features

- **Init** (Command Palette: `SketchPen: Init`) — scaffolds a new icon-set
  folder: a working `styles/default.globals`, a `templates/` folder with an
  example `.spt`, and a starter `icon.sp` that already renders something.
  Prompts for a target folder (defaults to the workspace root); never
  overwrites files that already exist, so it's safe to re-run on a partially
  set up folder.
- **Completion** — keyword/method completion (with parameter snippets) for
  `pen`/`brush`/`gradientbrush`/`line`/`circle`/`path`/`text`/`transform`/`globals`,
  filtered to what's actually valid in the current file type (`.sp`, `.spt`,
  or `.globals`), plus `@@variable` completion sourced from your project's
  styles folder `default.globals` (a local `styles/` subfolder by default, or
  wherever an optional `.sketchpen.json`'s `stylesPath` points).
- **Hover** — hovering an `@@name` reference shows the value declared for it
  in the folder's styles folder `default.globals` (a best-effort text match
  against the base file, not a compiled/resolved value — style overrides
  aren't reflected).
- **Live preview** — renders the active `.sp` file on save (SVG, via
  `sketchpen compose --composer svg`) into a preview panel, scaled to fill
  most of the panel. Also re-renders automatically when a `#include`d `.spt`
  file it (transitively) depends on is saved, as long as that icon's preview
  panel is open. The panel toolbar has a **Grid** toggle (the `-50..+50`
  logical coordinate square from
  [docs/SYNTAX.md](../docs/SYNTAX.md#coordinate-system), for alignment), a
  **Coordinates** toggle for a small `x,y` label at every grid intersection,
  and a **Style** dropdown to preview any named style from the icon's
  styles folder, not just the default.
- **Diagnostics** — compile errors surface as editor warnings, including
  inside `#include`d `.spt` files. Line-accurate; not column-accurate — see
  [docs/CLI.md](../docs/CLI.md#the-language-info-subcommand) for why.
- **Syntax highlighting** for keywords, strings, comments, numbers, and
  `@@variable` references.
- **CodeLens** — inline `Preview` / `Export SVG` / `Export PNG` links above
  every `.sp` file, next to the code.
- **Export commands**, reachable from three places — Command Palette
  (`SketchPen: Export as SVG` / `Export as PNG` / `Export Package…`), the
  editor title bar icon, and the Explorer right-click menu (`SketchPen`
  submenu on `.sp` files). `Export Package…` wraps every `compose` composer —
  batch ZIPs, web sprites, themeable CSS-variable HTML.
  - `Export as PNG` can optionally emit several resolutions at once (pixel-
    ratio multipliers of the chosen size, e.g. `1,2,3` for `@1x`/`@2x`/`@3x`),
    written into a chosen folder and named exactly like the CLI's own fixed
    matrix (`<name>_<size>@<ratio>.png` — see
    [docs/CLI.md](../docs/CLI.md#output-naming-and-location)). Leave it at the
    default `1` for the previous single-file behavior.
  - `Export Package…` exposes `--resolutions` (DPI values, e.g.
    `96,144,192`) for the two composers where it applies (`png-zip`,
    `web-sprite-zip`).
- **Getting Started walkthrough** (VS Code's own "Get Started" page) — walks
  through installing the CLI, scaffolding a project, previewing, and
  exporting.

## Known limitations

- `.spt` template files can't be previewed standalone (they're fragments
  included via `#include`, not complete icons) — preview a `.sp` file that
  includes them instead. Diagnostics for a `.spt` file's own errors still
  work correctly when the including `.sp` file is composed.
- Diagnostics point at the correct **line**, not a precise column/token span
  — the underlying compiler doesn't track character offsets today.

## Language reference

See [`docs/SYNTAX.md`](../docs/SYNTAX.md) in the main repo for the full `.sp`/
`.spt`/`.globals` syntax, and [`docs/CLI.md`](../docs/CLI.md) for the CLI this
extension drives.
