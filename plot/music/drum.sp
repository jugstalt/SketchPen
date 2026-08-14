// Drum sticks
#include "../styles/default.globals"
transform.reset();

line.draw(-30, -20, 30, 20, @@outlinePenColor);
line.draw(-30, 20, 30, -20, @@outlinePenColor);
circle.fill(7, -30, -20, @@brushColor);
circle.fill(7, 30, 20, @@brushColor);
circle.fill(7, -30, 20, @@brushColor);
circle.fill(7, 30, -20, @@brushColor);