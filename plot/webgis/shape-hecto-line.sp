path.begin();
path.addlines(-42,42, 20,0, 42,-42);
path.draw();

brush.color(@@penColor);
gradientbrush.color(@@penColor);

circle.fill(14,14,  -42,42);
circle.fill(14,14,  20,0);
circle.fill(14,14,  42,-42);

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);

transform.reset();
transform.translate(-39, 18);
#include "templates/x.spt"

transform.reset();
transform.translate(38, 5);
#include "templates/x.spt"

transform.reset();
transform.translate(18, -39);
#include "templates/x.spt"