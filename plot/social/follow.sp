// Follow (add user)
#include "../styles/default.globals"
transform.reset();
transform.translate(-8, 0);

circle.arc(200, 140, 60, 60, 0, 40);
circle.fill(40, 40, 0, -14);
circle.draw(40, 40, 0, -14, @@outlinePenColor);

transform.reset();
transform.translate(30, 6);
line.draw(0, -16, 0, 16, @@outlinePenColor);
line.draw(-16, 0, 16, 0, @@outlinePenColor);
