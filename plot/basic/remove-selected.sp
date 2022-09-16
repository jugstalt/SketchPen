transform.reset();
transform.translate(-30,-30);
transform.scale(0.3);
#include "templates/box-outline.spt"

transform.reset();
transform.translate(-30,-0);
transform.scale(0.3);
#include "templates/box-outline.spt"

pen.width(20);
path.begin();
path.addlines(-25,10, -9,25, 25,-25);
path.draw();
pen.width(@@penWidth);

transform.reset();
transform.translate(-30, 30);
transform.scale(0.3);
#include "templates/box-outline.spt"

transform.reset();
transform.translate(30,0);
transform.scale(0.5);
#include "templates/trashcan.spt"

// Arrows

transform.reset();
transform.rotate(180);
transform.scale(0.25);
#include "templates/arrow.spt"
