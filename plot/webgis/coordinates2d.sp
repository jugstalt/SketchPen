pen.color(@@outlinePenColor);
#include "templates/axis2d.spt"
pen.color(@@penColor);

transform.reset();
transform.translate(35,19);
#include "templates/x.spt"

transform.reset();
transform.translate(-25,-35);
#include "templates/y.spt"
