// Colors
// -------
// Every color parameter (pen.color, brush.color, gradientbrush.color, and
// the inline color overrides on line/circle/path/rect/text) accepts either
// an HTML hex string or R,G,B(,A) integers -- ParameterExtensions.ToColor
// picks the form based purely on how many arguments were given.

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
circle.fill(20, 0, 0, 230, 126, 34);      // R, G, B (each 0..255)

transform.reset();
transform.translate(0, 22);
circle.fill(20, 0, 0, 74, 144, 217, 120); // R, G, B, alpha

transform.reset();
