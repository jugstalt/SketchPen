path.begin();
path.addlines(-20,42, -42,42, -42,-42, 42,-42, 42,-15);
// path.close();
// path.fill();
path.draw(@@outlinePenColor);

brush.color(@@penColor);
gradientbrush.color(@@penColor);

circle.fill(14,14,  -42,42);
circle.fill(14,14,  -42,-42);
circle.fill(14,14,  42,-42);
// circle.fill(14,14,  42,42);

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);

transform.reset();
transform.translate(18, 18);
transform.scale(0.75);

#include "templates/binoculars.spt"
