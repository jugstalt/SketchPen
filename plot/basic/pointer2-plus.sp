transform.reset();
transform.translate(23,-20);
circle.fill(45,45, 0,0);
circle.draw(45,45, 0,0, @@outlinePenColor);
pen.width(@@penWidthBold);
line.draw(-12,0, 12,0);
line.draw(0,-12, 0,12);
pen.width(@@penWidth);

transform.reset();
transform.translate(-17,9);
transform.rotate(-30);
#include "templates/pointer2.spt"