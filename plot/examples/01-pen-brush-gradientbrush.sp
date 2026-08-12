// pen / brush / gradientbrush
// ----------------------------
// `pen` controls outlines drawn with `*.draw(...)`; `brush` controls fills
// drawn with `*.fill(...)`. Both are set once and stay in effect for every
// following draw/fill call until changed again -- there is no "per-call"
// pen/brush, only the current one (which individual calls can still
// override inline via a trailing color argument, see 02-line.sp).
//
// `gradientbrush` optionally replaces a plain `brush` fill with a two-color
// linear gradient, automatically, as soon as its color is set to something
// other than transparent -- no separate "use gradient" flag needed.
//
// This folder default pen/brush color already comes from _.globals
// (auto-included before every .sp file here, see 10-variables-and-globals.sp)
// -- this file overrides them locally to show each method explicitly.

pen.color("#222222");
pen.width(5);
pen.minwidth(1);      // never render thinner than 1px, even on tiny icons
pen.maxwidth(999);    // no practical upper bound
pen.cap("round");     // "round" | "flat" | "square" -- shape of line ends/joins

brush.color("#cfe3f7");
transform.reset();
transform.translate(-20, 0);
circle.fill(38);
circle.draw(38);

// gradientbrush: a second color + an axis (x1,y1 -> x2,y2) turns the next
// fill(s) into a linear gradient instead of the flat brush color above.
gradientbrush.color("#f7d774");
gradientbrush.points(-15, -15, 15, 15);
transform.reset();
transform.translate(20, 0);
circle.fill(38);
circle.draw(38);

transform.reset();
