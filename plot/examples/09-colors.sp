// Colors
// -------
// Every color parameter (pen.color, brush.color, gradientbrush.color, and
// the inline color overrides on line/circle/path/rect/text) accepts either
// an HTML hex string or R,G,B(,A) integers -- ParameterExtensions.ToColor
// picks the form based purely on how many arguments were given.
//
// R,G,B(,A) numeric colors are shown here via brush.color(...) rather than
// as an inline circle.fill(pos..., r,g,b) override: the circle position is
// itself 1-4 numbers, and since a numeric color has no type boundary to
// stop at (unlike a "#hex" string), a trailing R,G,B triple would be
// swallowed into the position instead -- a known, separate limitation of
// the position-detection logic, not something to route around here inline.

transform.reset();
transform.translate(-32, -20);
circle.fill(20, 0, 0, "#e0592a");         // 6-digit hex

transform.reset();
transform.translate(0, -20);
circle.fill(20, 0, 0, "#0a5");            // 3-digit hex shorthand

transform.reset();
transform.translate(32, -20);
circle.fill(20, 0, 0, "#4a90d9", 120);    // hex + alpha (0..255)

transform.reset();
transform.translate(-32, 22);
brush.color(230, 126, 34);                // R, G, B (each 0..255)
circle.fill(20);

transform.reset();
transform.translate(0, 22);
brush.color(74, 144, 217, 120);           // R, G, B, alpha
circle.fill(20);

transform.reset();
