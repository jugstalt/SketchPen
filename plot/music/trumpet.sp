// Trumpet
#include "../styles/default.globals"
transform.reset();

line.draw(-38, 12, 24, 12, @@outlinePenColor);
line.draw(-38, 20, 24, 20, @@outlinePenColor);

path.begin();
path.addlines(20, 4, 34, -2, 43, -2, 43, 34, 34, 34, 20, 28);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

line.draw(24, 12, 24, 20, @@outlinePenColor);
line.draw(12, 12, 12, 20, @@outlinePenColor);
line.draw(0, 12, 0, 20, @@outlinePenColor);
line.draw(-12, 12, -12, 20, @@outlinePenColor);

circle.fill(8, 12, 4, @@gradientbrushColor);
circle.fill(8, 0, 4, @@gradientbrushColor);
circle.fill(8, -12, 4, @@gradientbrushColor);

path.begin();
path.addlines(-38, 8, -46, 8, -46, 24, -38, 24);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);