// Notification (bell)
#include "../styles/default.globals"
transform.reset();

path.begin();
path.addarc(180, 180, 46, 40, 0, -6);
path.addlines(30, 22, -30, 22);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

path.begin();
path.addlines(-10, 22, 10, 22, 10, 28, -10, 28);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);

circle.fill(10, 22, -30, @@outlinePenColor);
