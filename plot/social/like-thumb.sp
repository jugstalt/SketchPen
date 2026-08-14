// Like (thumbs up, hand seen from the side)
#include "../styles/default.globals"
transform.reset();

path.begin();
path.addpoint(-26, 32);
path.addlines(-26, 32, -26, 4);
path.addcubic(-29, -8, -29, -20, -25, -28);
path.addcubic(-21, -37, -8, -37, -5, -28);
path.addcubic(-3, -22, -4, -17, -2, -12);
path.addcubic(7, -20, 20, -20, 25, -13);
path.addcubic(27, -11, 28, -9, 28, -4);
path.addlines(28, -4, 28, 28);
path.addcubic(28, 35, 24, 38, 18, 38);
path.addlines(18, 38, -20, 38);
path.addcubic(-24, 38, -26, 36, -26, 32);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

// curled fingers
line.draw(-12, 1, 28, 1, @@outlinePenColor);
line.draw(-12, 13, 28, 13, @@outlinePenColor);
line.draw(-12, 25, 28, 25, @@outlinePenColor);
