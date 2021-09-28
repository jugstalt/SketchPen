transform.rotate(90);
#include "templates/arrow-short.spt"

transform.reset();
pen.width(@@penWidthBold);

path.begin();

path.addlines(-30,17, -30,25);
path.addlines(30,25, 30,17);
path.draw();

