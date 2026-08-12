// #include
// ---------
// Textually splices another file (typically a .spt template, conventionally
// kept in a templates/ subfolder) in at exactly this point, before
// compilation -- the included and including file end up sharing the same
// pen/brush state, path state, and @@variables, since #include is plain
// text substitution, not a function call. See templates/badge-shape.spt.

#include "templates/badge-shape.spt"

// Content added AFTER the include, on top of what the template drew --
// this only works because the include happens textually before this line,
// exactly as if badge-shape.spt contents were pasted in here by hand.
text.draw("OK", 16, 0, 6, @@outlinePenColor);
