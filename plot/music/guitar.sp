// Guitar pick
#include "../styles/default.globals"
transform.reset();

path.begin();
path.addlines(0, -42, 30, 24, 0, 42, -30, 24);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

// Decorative center hole
path.begin();
path.addlines(0, -27, 12, 19, 0, 31, -12, 19);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);
circle.draw(9, 0, 13, @@outlinePenColor);

// Strings and frets
line.draw(-3, -43, -3, 30, @@outlinePenColor);
line.draw(0, -43, 0, 30, @@outlinePenColor);
line.draw(3, -43, 3, 30, @@outlinePenColor);
line.draw(-5, -31, 5, -31, @@outlinePenColor);
line.draw(-5, -23, 5, -23, @@outlinePenColor);
line.draw(-5, -15, 5, -15, @@outlinePenColor);