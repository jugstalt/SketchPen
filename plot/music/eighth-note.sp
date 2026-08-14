// Eighth note
#include "../styles/default.globals"
transform.reset();

line.draw(10, -35, 10, 22, @@outlinePenColor);
path.begin();
path.addlines(10, -35, 35, -39, 35, -31, 10, -27);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);
circle.fill(18, 0, 27, @@brushColor);
circle.draw(18, 0, 27, @@outlinePenColor);

line.draw(10, -4, -14, 15, @@outlinePenColor);
circle.fill(16, -20, 20, @@brushColor);
circle.draw(16, -20, 20, @@outlinePenColor);