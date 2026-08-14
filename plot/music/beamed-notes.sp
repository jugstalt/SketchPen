// Beamed eighth notes
#include "../styles/default.globals"
transform.reset();

line.draw(-22, -30, -22, 22, @@outlinePenColor);
line.draw(22, -30, 22, 22, @@outlinePenColor);
path.begin();
path.addlines(-22, -34, 22, -34, 22, -25, -22, -25);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);
path.begin();
path.addlines(-22, -25, 22, -25, 22, -17, -22, -17);
path.close();
path.fill(@@gradientbrushColor);
circle.fill(18, -22, 27, @@brushColor);
circle.draw(18, -22, 27, @@outlinePenColor);
circle.fill(18, 22, 27, @@brushColor);
circle.draw(18, 22, 27, @@outlinePenColor);