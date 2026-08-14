// Share (connected nodes)
#include "../styles/default.globals"
transform.reset();

line.draw(-24, 0, 22, -28, @@outlinePenColor);
line.draw(-24, 0, 22, 28, @@outlinePenColor);

circle.fill(20, -24, 0, @@brushColor);
circle.draw(20, -24, 0, @@outlinePenColor);
circle.fill(20, 26, -30, @@brushColor);
circle.draw(20, 26, -30, @@outlinePenColor);
circle.fill(20, 26, 30, @@brushColor);
circle.draw(20, 26, 30, @@outlinePenColor);
