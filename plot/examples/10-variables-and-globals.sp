// Variables (@@name) and .globals files / styling
// ---------------------------------------------------
// Anywhere a number/string parameter is expected, @@name can reference a
// value previously set via globals.set/globals.tryset in a loaded .globals
// file. This whole plot/examples/ folder auto-loads _.globals before every
// .sp file in it (see that file) -- that is where @@brushColor,
// @@outlinePenColor and @@accentColor below come from; @@name is replaced
// by the currently effective value at compile time, and compilation fails
// with "Unknown variable: name" if it was never set.
//
// _.globals (excerpt):
//   globals.tryset(brushColor, "#4a90d9");
//   globals.tryset(outlinePenColor, "#1a1a1a");
//   pen.color(@@penColor);
//   brush.color(@@brushColor);
//
// A second file, _dark.globals, defines the SAME names via `globals.set`
// (always overwrites) instead of `tryset` (only sets if not already
// present) -- since it is loaded BEFORE _.globals, its values "win" and
// _.globals tryset calls silently become no-ops. Rendering this whole
// folder with that style swaps every icon color without touching a
// single .sp file:
//   sketchpen compose plot/examples --composer png-zip --styles ,dark --sizes 64
// (legacy CLI syntax: SketchPen.exe plot/examples -custom_globals dark)

transform.reset();
transform.translate(-18, 0);
circle.fill(28, 0, 0, @@brushColor);
circle.draw(28, 0, 0, @@outlinePenColor);

transform.reset();
transform.translate(18, 0);
rect.fill(28, 28, 6, @@accentColor);
rect.draw(28, 28, 6, @@outlinePenColor);

transform.reset();
