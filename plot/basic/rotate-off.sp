pen.width(@@penWidthBold);

circle.arc(200,300, 75, 75, 0,0);
circle.draw(10,10, 0,0);

transform.translate(-35, -12);
transform.rotate(27);
path.begin();
path.addlines(-10,-10, 0,0, 10,-10);
path.draw();

pen.width(@@penWidth);

brush.color("#faa");
gradientbrush.color("#f00");
pen.color("#fff");

transform.reset();
transform.translate(20,20);
transform.scale(0.55);
#include "templates/round-x.spt"