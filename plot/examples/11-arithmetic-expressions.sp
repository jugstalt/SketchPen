// Arithmetic expressions  (new)
// -------------------------------
// Numeric parameters can be small expressions instead of only a bare
// literal or a bare @@name: `+ - * /` and parentheses, standard precedence
// (`*`/`/` before `+`/`-`). Only ever built when a parameter actually
// contains more than one token -- a bare literal/@@name behaves exactly as
// before this feature existed. Scope is deliberately minimal: numeric
// arithmetic only, no string concatenation, no comparisons, no functions.

// Classic motivating example: a "percent full" pie slice derived from ONE
// variable instead of a hand-computed angle literal per icon. Compare
// plot/basic/circle-pie-25.sp / -50.sp / -75.sp / -100.sp, which each
// hardcode a different literal sweep angle (-90 / -180 / -270 / -360) --
// this is the same shape, but parameterized by @@percent (from default.globals).
transform.reset();
transform.translate(-18, 0);
path.begin();
path.addlines(0,0, 15,0);
path.addarc(0, -3.6 * @@percent, 30,30, 0,0);   // -3.6 degrees per percent
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

// Subtraction needs no spaces around the "-": "@@x-10" and "@@x - 10"
// tokenize identically, since PreComplier strips all whitespace before
// compiling anyway. @@x is 40 (see default.globals), so this is circle center 30.
transform.reset();
transform.translate(18, -14);
circle.fill(14, @@x-10, 0, @@accentColor);

// A parenthesized sub-expression, mixing a literal and a variable.
transform.reset();
transform.translate(18, 14);
circle.fill(14, (@@x-10)/2, 0, @@accentColor);

transform.reset();
