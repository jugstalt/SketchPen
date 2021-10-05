line.draw(-42,20,  42,20);
line.draw(-42, 0, -42,40);
line.draw( 42, 0,  42,40);

brush.color(@@penColor);
gradientbrush.color(@@penColor);

circle.fill(14,14,  -42,20);
circle.fill(14,14,  42,20);

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);

#include "templates/x.spt"