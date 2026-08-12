// rect  (new)
// ------------
// An axis-aligned rectangle, optionally with rounded corners, always
// centered at (0,0) before any `transform` is applied -- see 08-transform.sp
// for how to move/rotate it. `rect` used to be a reserved-but-unimplemented
// keyword; rectangles had to be built via path.addlines + path.close()
// (still true for anything more exotic than a rounded corner, e.g. a
// trapezoid).

transform.reset();
transform.translate(-25, 0);
rect.fill(40, 40);          // width, height -- sharp corners
rect.draw(40, 40);

transform.reset();
transform.translate(25, 0);
rect.fill(40, 40, 10, "#f7d774");  // + cornerRadius, + inline fill color
rect.draw(40, 40, 10);             // + cornerRadius, outline uses current pen

transform.reset();
