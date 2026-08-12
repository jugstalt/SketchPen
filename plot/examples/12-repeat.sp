// repeat(n) { ... }  (new)
// ---------------------------
// Textually duplicates a block of statements n times, before compilation
// -- a preprocessor macro, exactly like #include, with no loop-index
// variable. Most useful together with a cumulative transform.rotate: a
// block that rotates a little further each time it runs replaces what
// would otherwise be n near-identical copy-pasted statements. Compare
// plot/basic/templates/rack-wheel.spt, which used to copy-paste its
// 5-statement gear-tooth block 8 times by hand.
//
// The `repeat(n) {` header and the closing `}` must each be alone on their
// own line.

transform.reset();
repeat(10) {
    transform.rotate(36);
    circle.fill(8, 0, -32, @@brushColor);
    circle.draw(8, 0, -32, @@outlinePenColor);
}
transform.reset();

circle.fill(14, 0, 0, @@accentColor);
