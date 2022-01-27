transform.rotate(45);

#include "resize-width.sp"
#include "resize-height.sp"

brush.color("#faa");
gradientbrush.color("#f00");
pen.color("#fff");

transform.reset();
transform.translate(20,20);
transform.scale(0.55);
#include "templates/round-x.spt"