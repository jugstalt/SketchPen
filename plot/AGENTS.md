# SketchPen icons -- agent instructions

This folder contains SketchPen icon scripts (`.sp`), reusable templates
(`.spt`, included via `#include`), and a `styles/` folder with `.globals`
color themes. SketchPen compiles these to PNG or SVG.

Full reference: <https://github.com/jugstalt/SketchPen/blob/main/docs/SYNTAX.md>
(language) and <https://github.com/jugstalt/SketchPen/blob/main/docs/CLI.md>
(CLI). This file is a condensed cheat sheet, not a replacement.

## Workflow: render, then look

**Always render and view your icon before considering it done -- do not judge
an icon purely by reading the code.**

```bash
sketchpen compose <file>.sp --composer png --sizes 128 --out /tmp/preview.png
```

Then open/view `/tmp/preview.png`. Iterate: edit the `.sp` file, re-render,
look again, until it actually looks right. A non-zero exit code plus stderr
means a compile error (wrong syntax, undefined `@@variable`, etc.) -- fix
that before re-rendering.

## Coordinate system

Every icon is drawn in a logical **-50..+50 square**, origin at the center,
X right, Y **down** (screen convention). Actual pixel size is applied later
(`--sizes`) -- write shapes in this -50..+50 space regardless of target size.

## Statement syntax

```
keyword.method(param1, param2, ...);
```

One statement per line, semicolon optional (auto-appended), spaces
irrelevant outside `"..."` strings. Colors: `"#ff0000"`, `"#f00"`,
`"#ff0000", 128` (alpha), or `255, 0, 0[, 128]` (RGB[A]).

**IMPORTANT: never put an apostrophe inside a `//` comment** (e.g. "don't",
"it's") -- a known lexer limitation crashes compilation on it. Rephrase
instead (e.g. "do not", "it is").

**IMPORTANT: for `circle.*`, prefer a hex-string color** (`"#rrggbb"`) over
numeric `r,g,b(,a)` when passing it inline after `pos...`. `pos...` is
itself 1-4 numbers, and a numeric color has no type boundary to stop at --
it silently gets swallowed into the position instead, unless `pos` already
uses its full 4 values. For a numeric color, call
`brush.color(r,g,b[,a])`/`pen.color(r,g,b[,a])` on its own line right
before the `circle.*` call instead.

## Commands (all take a logical -50..+50 position unless noted)

- `pen.color(c)` / `.width(w)` / `.minwidth(w)` / `.maxwidth(w)` /
  `.cap("round"|"flat"|"square")` -- outline style for `*.draw(...)`.
- `brush.color(c)` -- fill color for `*.fill(...)`.
- `gradientbrush.color(c)` + `.points(x1,y1, x2,y2)` -- adds a linear
  gradient to the next fill once its color is non-transparent.
- `line.draw(x1,y1, x2,y2[, color[, width]])`
- `rect.draw/fill(width, height[, cornerRadius[, color]])` -- centered at
  `(0,0)` before any transform.
- `circle.draw/fill(pos...[, color])`, `circle.arc/pie(startAngle,
  sweepAngle, pos...[, color])` -- `pos` is 1-4 numbers: `diameter` /
  `diameterX,diameterY` / `diameter,x,y` / `diameterX,diameterY,x,y`
  (always centered, never top-left). Angles: 0 deg = right (+X), positive =
  clockwise.
- `path.begin()` (new path) / `.start()` (new subfigure, same path) /
  `.addlines(x1,y1, x2,y2, ...)` / `.addarc(startAngle, sweepAngle,
  pos...)` / `.addpoint(x,y)` / `.addcubic(cp1x,cp1y, cp2x,cp2y, x,y)`
  (cubic Bezier) / `.addquad(cpx,cpy, x,y)` (quadratic Bezier) / `.close()`
  (line back to start) / `.draw([color])` / `.fill([color])`.
- `text.draw(text, size, x, y[, color])`
- `transform.translate(x,y)` / `.rotate(angle[, pivotX, pivotY])` /
  `.scale(ratio)` or `.scale(ratioX, ratioY)` / `.reset()` -- cumulative
  until `reset()`; typically used around `#include` to place/repeat a
  template at different positions/angles.
- `globals.set(name, value)` / `.tryset(name, value)` (tryset = only if
  unset) -- **`.globals` files only**.

`@@name` anywhere a parameter is expected reads a globals variable.
Arithmetic (`+ - * /`, parens, e.g. `-3.6 * @@percent`) works in any
numeric parameter.

## Reuse: `#include` and `repeat`

`#include "templates/foo.spt"` textually inlines a template (path relative
to the including file). `repeat(n) { ...statements... }` (header and `}`
each alone on their own line, `n` a literal integer) unrolls a block `n`
times -- combine with cumulative `transform.rotate` for radial patterns
instead of copy-pasting `n` near-identical blocks.

## Styling

`styles/default.globals` is always loaded first. Render with a named style
via `-style <name>` (root command) or `--styles <name>` (compose),
loading `styles/<name>.globals` first (its `set` calls win over
`default.globals`'s `tryset` calls). List available styles: look at the
`.globals` files in `styles/` (or wherever `.sketchpen.json`'s
`stylesPath` points, if present).

## Minimal worked example

```csharp
// A filled circle with an outlined ring
circle.fill(70, "#4a90d9");
circle.draw(70);
circle.draw(30, "#fff");
```

## Conventions for this folder

- New icons: one `.sp` file per icon, in this folder.
- Reusable shapes: `.spt` templates under `templates/`, included via
  `#include`.
- Match the pen width / color style already used by existing icons in this
  folder (open a couple of the existing `.sp` files and `styles/*.globals`
  for reference before inventing new values).
