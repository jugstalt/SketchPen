// Comment (chat bubble)
#include "../styles/default.globals"
transform.reset();

path.begin();
path.addlines(-38, -30, 38, -30, 38, 18, -12, 18, -22, 34, -18, 18, -38, 18);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

circle.fill(6, -18, -6, @@outlinePenColor);
circle.fill(6, 0, -6, @@outlinePenColor);
circle.fill(6, 18, -6, @@outlinePenColor);
