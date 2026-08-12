// Comments
// ---------
// Line comments start with `//` and run to the end of the line. This
// entire file is (almost) nothing but comments -- comments are preserved
// through #include/repeat expansion (each unrolled/included copy keeps its
// own comments), which is also how the compiler tracks which original file
// a compile error came from.

circle.fill(30, 0, 0, @@brushColor);   // trailing comments work too
circle.draw(30, 0, 0, @@outlinePenColor);
