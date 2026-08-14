// Like (heart)
#include "../styles/default.globals"
transform.reset();

path.begin();
path.addarc(180, 180, 30, 30, -15, -14);
path.addarc(180, 180, 30, 30, 15, -14);
path.addcubic(28, 10, 8, 30, 0, 36);
path.addcubic(-8, 30, -28, 10, -30, -14);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);
