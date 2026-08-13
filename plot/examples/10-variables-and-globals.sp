// Variables (@@name) and .globals files / styling
// ---------------------------------------------------
// Anywhere a number/string parameter is expected, @@name can reference a
// value previously set via globals.set/globals.tryset in a loaded .globals
// file. Every icon-set folder has a *styles folder* -- by default a local
// styles/ subfolder, or wherever an optional .sketchpen.json names via
// "stylesPath" (see docs/SYNTAX.md > ".globals files and styling"; can even
// be a location shared across several icon sets). This folder has no
// .sketchpen.json, so it uses styles/ -- default.globals there is
// auto-loaded before every .sp file in this directory (see that file) --
// that is where @@brushColor, @@outlinePenColor and @@accentColor below
// come from; @@name is replaced by the currently effective value at compile
// time, and compilation fails with "Unknown variable: name" if it was never
// set.
//
// styles/default.globals (excerpt):
//   globals.tryset(brushColor, "#4a90d9");
//   globals.tryset(outlinePenColor, "#1a1a1a");
//   pen.color(@@penColor);
//   brush.color(@@brushColor);
//
// A second file, styles/dark.globals, defines the SAME names via
// `globals.set` (always overwrites) instead of `tryset` (only sets if not
// already present) -- since it is loaded BEFORE default.globals, its values
// "win" and the tryset calls in default.globals silently become no-ops.
// Rendering this whole folder with that style swaps every icon color
// without touching a single .sp file:
//   sketchpen compose plot/examples --composer png-zip --styles ,dark --sizes 64
// (classic CLI syntax: SketchPen.exe plot/examples -style dark)

transform.reset();
transform.translate(-18, 0);
circle.fill(28, 0, 0, @@brushColor);
circle.draw(28, 0, 0, @@outlinePenColor);

transform.reset();
transform.translate(18, 0);
rect.fill(28, 28, 6, @@accentColor);
rect.draw(28, 28, 6, @@outlinePenColor);

transform.reset();
