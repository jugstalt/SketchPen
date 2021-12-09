path.begin();
path.addlines(-30,-45, 30,-45, 30,45, -30,45);
path.close();
path.fill("#fff");
path.draw();

line.draw(-20,-30, 20,-30);

path.begin();
path.addlines(-25,-20, 25,-20, 25,0, -25,0);
path.fill(@@penColor);
line.draw(-20,-10, 20,-10, "#fff");

line.draw(-20, 10, 20, 10);
line.draw(-20, 30, 20, 30);

transform.reset();
transform.translate(20,14);
transform.rotate(-45);
transform.scale(0.7);
pen.width(@@penWidthBold);

#include "templates/arrow-pointer.spt"