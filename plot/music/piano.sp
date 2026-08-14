// Piano keys
#include "../styles/default.globals"
transform.reset();

// Keyboard frame
path.begin();
path.addlines(-43, -17, 43, -17, 43, 17, -43, 17);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

// White keys
path.begin();
path.addlines(-39, -12, 39, -12, 39, 12, -39, 12);
path.close();
path.fill(@@backgroundBrushColor);
path.draw(@@outlinePenColor);
line.draw(-29, -12, -29, 12, @@outlinePenColor);
line.draw(-19, -12, -19, 12, @@outlinePenColor);
line.draw(-9, -12, -9, 12, @@outlinePenColor);
line.draw(1, -12, 1, 12, @@outlinePenColor);
line.draw(11, -12, 11, 12, @@outlinePenColor);
line.draw(21, -12, 21, 12, @@outlinePenColor);
line.draw(31, -12, 31, 12, @@outlinePenColor);

// Black keys
path.begin();
path.addlines(-35, -12, -29, -12, -29, 3, -35, 3);
path.close();
path.fill(@@gradientbrushColor);
path.begin();
path.addlines(-25, -12, -19, -12, -19, 3, -25, 3);
path.close();
path.fill(@@gradientbrushColor);
path.begin();
path.addlines(-15, -12, -9, -12, -9, 3, -15, 3);
path.close();
path.fill(@@gradientbrushColor);
path.begin();
path.addlines(5, -12, 11, -12, 11, 3, 5, 3);
path.close();
path.fill(@@gradientbrushColor);
path.begin();
path.addlines(15, -12, 21, -12, 21, 3, 15, 3);
path.close();
path.fill(@@gradientbrushColor);
path.begin();
path.addlines(25, -12, 31, -12, 31, 3, 25, 3);
path.close();
path.fill(@@gradientbrushColor);
path.begin();
path.addlines(35, -12, 39, -12, 39, 3, 35, 3);
path.close();
path.fill(@@gradientbrushColor);
