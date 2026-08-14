// Like (thumbs up)
#include "../styles/default.globals"
transform.reset();

path.begin();
path.addlines(-22, 42, 22, 42, 22, -8, -2, -8, -2, -20, -10, -34);
path.addarc(-30, -150, 16, 16, -17, -30);
path.addlines(-30, -14, -22, -8);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

line.draw(-14, 10, 14, 10, @@outlinePenColor);
line.draw(-14, 22, 14, 22, @@outlinePenColor);
line.draw(-14, 34, 14, 34, @@outlinePenColor);
