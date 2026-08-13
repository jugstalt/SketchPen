// line
// -----
// A single straight segment, stroked with the current pen (color/width set
// via `pen.*`, see 01-pen-brush-gradientbrush.sp), or with an inline
// override that only affects this one call. For multi-segment polylines,
// see path.addlines in 05-path-basics.sp.

line.draw(-40, -30, 40, -30);              // uses the current pen (from default.globals)
line.draw(-40, 0, 40, 0, "#e0592a");       // inline color override
line.draw(-40, 30, 40, 30, "#e0592a", 8);  // inline color + width override
