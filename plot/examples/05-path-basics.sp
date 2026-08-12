// path (straight segments + circular arcs)
// ------------------------------------------
// A composite outline/fill made of connected line segments (`addlines`) and
// circular arcs (`addarc`), either left open (a stroked polyline) or turned
// into a fillable polygon via `close()` (draws a final segment back to the
// start point). There is always only ONE "current" path -- `path.begin()`
// discards whatever came before it, and is also created implicitly the
// first time any other `path.*` call happens without a preceding `begin()`.
// See 06-path-bezier.sp for curved segments.

// A filled + outlined triangle (closed polygon)
transform.reset();
transform.translate(-22, 0);
path.begin();
path.addlines(0,-24, 20,14, -20,14);
path.close();
path.fill("#cfe3f7");
path.draw();

// An open path made of two arcs + a straight line (a dollar-sign "S" shape,
// stroke only) -- see plot/basic/templates/cash.spt for the original this
// is based on
transform.reset();
transform.translate(22, 0);
transform.scale(0.45);
path.begin();
path.addarc(-20, -230, 45,35, 0, -18);
path.addarc(270, 230, 60,40, 0, 20);
path.draw("#e0592a", 6);
path.begin();
path.addlines(-7,45, -7,-45);
path.draw("#e0592a", 6);

transform.reset();
