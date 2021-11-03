transform.reset();
transform.translate(-15,0);

#include "templates/bulb.spt"

brush.color("#faa");
gradientbrush.color("#f00");
pen.color("#fff");

transform.reset();
transform.translate(20,20);
transform.scale(0.55);
#include "templates/round-x.spt"