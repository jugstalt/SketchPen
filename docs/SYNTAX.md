# SketchPen Language Reference (`.sp` / `.spt` / `.globals`)

This page describes the scripting language used to define SketchPen icons:
file types, coordinate system, all commands (`pen`, `brush`, `gradientbrush`, `line`,
`rect`, `circle`, `path`, `text`, `transform`, `globals`), variables, includes, and
styling via `.globals` files.

Reference implementation in code:
[`SketchPenSyntax.cs`](../src/SketchPen.Parse/Lexer/SketchPenSyntax.cs),
[`Commands/`](../src/SketchPen.Plot/Commands),
[`ParameterExtensions.cs`](../src/SketchPen.Plot/Extensions/ParameterExtensions.cs),
[`PreComplier.cs`](../src/SketchPen.Plot/Compile/PreComplier.cs).

## Contents

- [File types](#file-types)
- [Basic syntax](#basic-syntax)
- [Coordinate system](#coordinate-system)
- [Position and point parameters](#position-and-point-parameters)
- [Colors](#colors)
- [Command reference](#command-reference)
  - [`pen`](#pen)
  - [`brush`](#brush)
  - [`gradientbrush`](#gradientbrush)
  - [`line`](#line)
  - [`rect`](#rect)
  - [`circle`](#circle)
  - [`path`](#path)
  - [`text`](#text)
  - [`transform`](#transform)
  - [`globals`](#globals)
- [Variables (`@@name`)](#variables-name)
- [`.globals` files and styling](#globals-files-and-styling)
- [`#include`](#include)
- [Comments](#comments)
- [Full example](#full-example)

## File types

| Extension  | Type                     | Purpose |
|------------|--------------------------|-------|
| `.sp`      | **Code file** (icon)    | A finished, independently renderable icon. One `.sp` file per `plot` directory corresponds to exactly one generated PNG. |
| `.spt`     | **Template**             | A reusable snippet (e.g. a shape) included via `#include` into `.sp` or other `.spt` files. Not rendered directly. |
| `.globals` | **Globals/style file**  | Defines named variables (colors, pen widths, …) referenced via `@@name`. Controls the "theme"/color style of an icon set. |

This mapping is represented in code via `EditorFileType` (`Code`, `Template`,
`Globals`), see [`Enums.cs`](../src/SketchPen.Plot/Enums.cs) and
[`SketchPenCodeService.GetEditorFileType`](../src/SketchPen.Code/Services/SketchPenCodeService.cs).
Every command "knows" via `[PlotCommandSupportedFileTypes]` in which file types it is
allowed (e.g. `globals.set(...)` is only valid in `.globals` files, `text.draw(...)`
only in `.sp`/`.spt`).

An icon set lives in its own directory under `plot/`, e.g.
[`plot/basic`](../plot/basic) or [`plot/webgis`](../plot/webgis). By convention,
templates live in a `templates/` subfolder inside that directory.

## Basic syntax

A SketchPen file consists of a sequence of **statements** of the form

```
keyword.method(param1, param2, ...);
```

- `keyword` is one of the reserved keywords (`pen`, `brush`, `gradientbrush`,
  `line`, `circle`, `path`, `text`, `transform`, `globals`).
- `method` is the method name of the respective command (e.g. `color`, `draw`, `fill`).
- Parameters are comma-separated; numbers, strings (`"..."`), or variables (`@@name`).
- Every statement ends with `;`. The compiler automatically appends a missing `;` at
  the end of a line (see `PreComplier`), so omitting it is harmless — but setting it
  explicitly is still good style.
- Spaces and tabs within a line are irrelevant and are stripped before compiling
  (exception: inside string literals `"..."`).
- Case does not matter for method names (`Draw()` == `draw()`); keywords themselves
  are currently used lowercase.

```csharp
pen.color("#ff0000");
pen.width(6);
circle.draw(50, 0, 0);
```

## Coordinate system

SketchPen always draws inside a **logical 100×100-unit square** with the origin at
the center, independent of the eventual pixel size of the PNG:

```
                        -50
                         ^
                         |
                         |
                    -50 <-- 0/0 --> +50
                         |
                         |
                         v
                        +50
```

- X grows to the right, Y grows **downward** (screen convention).
- The visible area roughly spans `-50` to `+50` in both directions.
- When actually rendering (`Plotter.Plot(width, height)` resp. `PlotContext.Init`),
  every coordinate is scaled by the factor `width / 100` (`PlotContext.Project`). An
  icon script is therefore **resolution-independent**: the same `.sp` script produces
  16×16, 32×32, or 512×512 px PNGs simply by specifying a different target size.
- `transform.translate/rotate/scale/reset` changes the coordinate system for all
  subsequent drawing commands (see [`transform`](#transform)).

## Position and point parameters

Many commands (`circle`, `path.addlines`, `path.addpoint`, `line.draw`,
`path.addarc`) expect position values as a plain list of numbers. The number of
values determines the meaning:

**Rectangle/ellipse position** (used by `circle.*`, `path.addarc`) – center point +
diameter, `ParameterExtensions.ToRectPos`:

| Number of values | Meaning                                    |
|--------------|-----------------------------------------------|
| 1            | `diameter` (width = height, center at 0,0)   |
| 2            | `diameterX, diameterY` (center at 0,0) |
| 3            | `diameter, x, y`                            |
| 4            | `diameterX, diameterY, x, y`             |

All four variants for a 50-unit circle at the center are equivalent:

```csharp
circle.fill(50);
circle.fill(50, 0, 0);
circle.fill(50, 50);
circle.fill(50, 50, 0, 0);
```

Circles/ellipses and rectangles are therefore always positioned **by their center
point**, not by their top-left corner.

**Point lists** (used by `line.draw`, `path.addlines`, `path.addpoint`) – every group
of 2 values is a point `(x, y)`, `ParameterExtensions.ToPoints`:

```csharp
line.draw(-45,0, 40,0);                 // line from (-45,0) to (40,0)
path.addlines(-7,45, -7,-45, 7,-45);    // polyline through 3 points
```

## Colors

Colors are specified either as an HTML color string or as RGB(A) integers
(`ParameterExtensions.ToColor`):

```csharp
pen.color("#ff0000");            // 6-digit hex code
pen.color("#f00");               // 3-digit hex code (shorthand)
pen.color("#ff0000", 128);       // hex code + alpha (0..255)
pen.color(255, 0, 0);            // R, G, B (each 0..255)
pen.color(255, 0, 0, 128);       // R, G, B, alpha
```

Without parameters (e.g. `path.draw()`), the color last set via `pen.color(...)` /
`brush.color(...)` is used. If `pen.color` is never set, the pen color is transparent
(the line is not drawn).

## Command reference

### `pen`

Pen properties for outlines (`*.draw(...)`).
Valid in `.globals`, `.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `color` | `pen.color(color)` / `pen.color(color, alpha)` / `pen.color(r,g,b[,a])` | Set the pen color. |
| `width` | `pen.width(width)` | Line width in logical units (scaled along with everything else when rendering). |
| `minwidth` | `pen.minwidth(minWidth)` | Lower bound for the actually rendered line width (px), prevents invisibly thin lines on small icons. |
| `maxwidth` | `pen.maxwidth(maxWidth)` | Upper bound for the rendered line width. |
| `cap` | `pen.cap("round"\|"flat"\|"square")` | Shape of line ends/joins. |

```csharp
pen.color("#000");
pen.width(6);
pen.minwidth(1);
pen.maxwidth(999);
pen.cap("round");
```

### `brush`

Fill color for `*.fill(...)`. Valid in `.globals`, `.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `color` | `brush.color(color)` | Set the fill color (same color notation as `pen.color`). |

### `gradientbrush`

Second color + gradient axis for a linear gradient that is automatically used
instead of a plain `brush` fill as soon as a `gradientbrush.color` other than
transparent is set. Valid in `.globals`, `.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `color` | `gradientbrush.color(color)` | Second gradient color. |
| `points` | `gradientbrush.points(x1,y1, x2,y2)` | Start/end point of the gradient axis (default: from top-left to bottom-right of the canvas). |

### `line`

A simple straight line. Valid in `.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `draw` | `line.draw(x1,y1, x2,y2[, color[, width]])` | Draws a line with the current pen or an optionally overridden color/width. |

```csharp
line.draw(-45, 0, -10, 0);
```

### `rect`

An axis-aligned rectangle, optionally with rounded corners, centered at
`(0,0)` before any `transform`. Valid in `.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `draw` | `rect.draw(width, height[, cornerRadius[, color]])` | Draw the outline (pen). |
| `fill` | `rect.fill(width, height[, cornerRadius[, color]])` | Fill the area (brush). |

```csharp
rect.fill(60, 40);                    // centered, 60x40
rect.fill(60, 40, 10);                // + rounded corners
rect.draw(60, 40, 10, "#ff0000");     // + outline color override
```

### `circle`

Circle/ellipse, either as a full circle, circular arc (outline only), or pie
segment (filled). Valid in `.spt`, `.sp`. For position parameters see
[Position and point parameters](#position-and-point-parameters).

| Method | Signature | Description |
|---|---|---|
| `draw` | `circle.draw(pos...[, color[, width]])` | Draw the outline (pen). |
| `fill` | `circle.fill(pos...[, color[, gradientColor]])` | Fill the area (brush). |
| `arc` | `circle.arc(startAngle, sweepAngle, pos...[, color[, width]])` | Circular arc (outline only) from `startAngle` over `sweepAngle` degrees. |
| `pie` | `circle.pie(startAngle, sweepAngle, pos...[, color[, gradientColor]])` | Filled circular segment ("pie slice"). |

`pos...` is 1–4 numbers as described above. For `arc`/`pie`, `startAngle` and
`sweepAngle` come **before** the position parameters. Angles: `0°` points to the
right (+X); positive angles rotate clockwise (Y points down).

```csharp
circle.fill(50);                          // full circle, diameter 50, center 0/0
circle.draw(50, 50, 0, -15, "#000");       // outline, 50x50, center (0,-15), color overridden
circle.arc(200, 140, 80, 80, 0, 50);       // arc from 200° over 140°, 80x80, center (0,50)
circle.pie(0, -180, 90, 90, 0, 0);         // filled segment
```

`circle-pie-*.sp` in [`plot/basic`](../plot/basic) demonstrates quarter/half/full
circle fill levels via `path.addarc` + `path.fill` as an alternative to `circle.pie`.

### `path`

A composite line/outline made of straight segments and arcs, either open
(polyline) or closed (polygon), stroked and/or filled. Valid in `.spt`, `.sp`.
**There is always only one "current" path** – every `path.begin()` discards the
previous one.

| Method | Signature | Description |
|---|---|---|
| `begin` | `path.begin()` | Start a new path (discards any still-open path). Implicitly needed at the start of each `.sp`/`.spt` file, but is also created automatically as soon as the first other `path.*` call occurs. |
| `start` | `path.start()` | Start a new **subfigure** within the same path, without closing the previous one (e.g. for multiple disconnected segments to be drawn with a single call, see `text.spt`). |
| `addlines` | `path.addlines(x1,y1, x2,y2, ...)` | Append one or more line segments to the current subfigure. |
| `addarc` | `path.addarc(startAngle, sweepAngle, diameter)` / `(startAngle, sweepAngle, diameterX, diameterY)` / `(startAngle, sweepAngle, diameter, x, y)` / `(startAngle, sweepAngle, diameterX, diameterY, x, y)` | Append a circular arc to the current subfigure. |
| `addpoint` | `path.addpoint(x, y)` | Append a single point. |
| `addcubic` | `path.addcubic(cp1x,cp1y, cp2x,cp2y, x,y)` | Append a cubic Bezier curve from the current point through two control points to `(x,y)`. Requires a current point (from a prior `addpoint`/`addlines`/`addarc`). |
| `addquad` | `path.addquad(cpx,cpy, x,y)` | Append a quadratic Bezier curve from the current point through one control point to `(x,y)`. Requires a current point. |
| `close` | `path.close()` | Close the current subfigure (line back to the start point), turning it into a polygon. |
| `draw` | `path.draw()` / `path.draw(color)` | Stroke the path with the current or the given pen. |
| `fill` | `path.fill()` / `path.fill(color)` | Fill the path with the current or the given brush. |

```csharp
// Closed triangle, filled + outlined
path.begin();
path.addlines(0,-35, 45,35, -45,35);
path.close();
path.fill("#ff0000");
path.draw();

// Open path made of arcs + a straight line, outline only (e.g. dollar sign "S")
path.begin();
path.addarc(-20, -230, 45, 35, 0, -18);
path.addarc(270, 230, 60, 40, 0, 20);
path.draw();

// Open path with a smooth curve (cubic Bezier)
path.begin();
path.addpoint(-30, 0);
path.addcubic(-10,-40, 10,-40, 30,0);
path.draw();
```

### `text`

Draw text at a position. Valid in `.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `draw` | `text.draw(text, size, x, y[, color])` | `text` as a string, `size` as font size in logical units, `(x,y)` as the reference point. |

```csharp
text.draw("42", 30, 0, 10, "#000");
```

### `transform`

Changes the coordinate system for all subsequent drawing commands (translation,
rotation, and scaling are cumulative until `transform.reset()` is called). Valid in
`.spt`, `.sp`.

| Method | Signature | Description |
|---|---|---|
| `translate` | `transform.translate(x, y)` | Shift the origin. |
| `rotate` | `transform.rotate(angle)` / `transform.rotate(angle, pivotX, pivotY)` | Rotate by `angle` degrees (clockwise), around the origin or around `(pivotX,pivotY)`. |
| `scale` | `transform.scale(ratio)` / `transform.scale(ratioX, ratioY)` | Scale uniformly or per axis. |
| `reset` | `transform.reset()` | Return to the initial state (origin at the center of the canvas, no rotation/scaling). |

```csharp
transform.reset();
transform.translate(21, 21);
transform.scale(0.6);
transform.rotate(22);
#include "templates/rack-wheel.spt"
```

`transform` is typically used to include a template multiple times at different
positions/sizes/rotation angles within a composite icon (see
[`admin.sp`](../plot/basic/admin.sp)).

### `globals`

Defines variables that can be read via `@@name`. Only valid in `.globals` files.

| Method | Signature | Description |
|---|---|---|
| `set` | `globals.set(name, value)` | Set a variable (overwrites any existing value). |
| `tryset` | `globals.tryset(name, value)` | Only set a variable if it does not yet exist – useful in the base `_.globals`, so a style-specific `_<style>.globals` that already set the same name earlier keeps its value. |

`name` is written unquoted as an identifier (no `"..."`), `value` is a number,
string, or bool depending on usage.

```csharp
globals.tryset(penColor, "#000");
globals.tryset(penWidth, 6);
```

## Variables (`@@name`)

Anywhere a parameter is expected, a variable previously defined via
`globals.set`/`globals.tryset` can be referenced instead of a literal:

```csharp
pen.color(@@penColor);
pen.width(@@penWidth);
path.fill(@@fillcolor);
```

During compilation, `@@name` is replaced by the currently effective value from the
loaded `.globals` files. If the variable is not defined, compilation fails with an
error (`Unknown variable: name`).

## `.globals` files and styling

`.globals` files make it possible to **render the same icon set in multiple color
styles** without touching the `.sp`/`.spt` scripts themselves. Convention per icon
set directory:

- `_.globals` – base/default values, **always** automatically included before every
  `.sp` file in the same directory (if present). Usually uses `tryset`, so that an
  additionally loaded custom style takes precedence.
- `_<stylename>.globals` – a named style, e.g. `_bg-dark.globals` or `_e.globals`.
  Included **additionally, before** `_.globals`, when the renderer is invoked with
  this style name (CLI: `-custom_globals <stylename>`, web: query parameter
  `globals`). Since it comes before `_.globals` and `tryset` in `_.globals` does not
  overwrite anything, the values from the custom style "win".

Order of assembly for a file (see `PreComplier`):

```
1. _<stylename>.globals   (if given and present)
2. _.globals               (if present, unless the target file is _.globals itself)
3. <name>.sp / <name>.spt  (the actual file)
```

Example from [`plot/basic`](../plot/basic):

```csharp
// _.globals – base values
globals.tryset(penColor, "#000");
globals.tryset(outlinePenColor, "#000");
globals.tryset(brushColor, "#fff");
globals.tryset(penWidth, 6);
pen.color(@@penColor);
pen.width(@@penWidth);
brush.color(@@brushColor);

// _bg-dark.globals – dark-mode style
globals.set(penColor, "#4cc2ff");
globals.set(outlinePenColor, "#fefefe");
globals.set(brushColor, "#444");
```

If `admin.sp` is rendered with the `bg-dark` style, the effective result is:

```csharp
// from _bg-dark.globals
globals.set(penColor, "#4cc2ff");
globals.set(outlinePenColor, "#fefefe");
globals.set(brushColor, "#444");
// ... (further values from _bg-dark.globals)

// from _.globals (tryset has no effect here anymore, values are already set)
globals.tryset(penColor, "#000");     // no-op, penColor is already "#4cc2ff"
...
pen.color(@@penColor);                // -> "#4cc2ff"
pen.width(@@penWidth);
brush.color(@@brushColor);            // -> "#444"

// contents of admin.sp
transform.reset();
...
```

## `#include`

Textually includes another file (typically a `.spt` template) at exactly this
point:

```csharp
#include "templates/rack-wheel.spt"
```

- The path is relative to the directory of the including file; as a fallback, a path
  relative to the working directory is also tried.
- Quotes are optional (`#include templates/x.spt` works too).
- Includes can be nested (a `.spt` can itself include further `.spt` files).
- Since `#include` is plain text substitution **before** actual compilation, the
  included and including file share the same `path` state, the same `pen`/`brush`
  settings, and any `@@variables` already defined.

## Comments

Line comments with `//`, to the end of the line:

```csharp
// This is a comment
pen.color("#000");  // color: black
```

## Full example

`cash.spt` – a dollar sign as a path made of two arcs ("S" shape) plus two vertical
strokes, rendered with the pen/brush from the previously loaded `.globals`:

```csharp
// cash.spt

path.begin();
path.addarc(-20, -230, 45,35, 0, -18);
path.addarc(270, 230, 60,40, 0, 20);

path.draw();

path.begin();
path.addlines(-7,45, -7,-45);
path.draw();

path.begin();
path.addlines(7,45, 7,-45);
path.draw();
```

Included via an associated `.sp` file, e.g.:

```csharp
// dollar.sp
#include "templates/cash.spt"
```

More real-world examples: [`plot/basic`](../plot/basic) (general UI icons),
[`plot/webgis`](../plot/webgis) (GIS-specific icons),
[`plot/gview`](../plot/gview) (axis/coordinate system symbols).
