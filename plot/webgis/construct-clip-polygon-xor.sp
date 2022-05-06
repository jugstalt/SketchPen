path.begin();
path.addlines(-45,-20, -20,-20, -20,20, -45,20);
path.fill("#75aaff");

path.begin();
path.addlines(45,-20, 20,-20, 20,20, 45,20);
path.fill("#75aaff");

path.begin();
path.addlines(-10,-20, 10,-20, 10,20, -10,20);
path.fill("#75aaff");

path.begin();
path.addlines(-20,-45, 20,-45, 20,-20, 10,-20, 10,-30, -10,-30, -10,-20, -20,-20);
path.fill("#75aaff");

path.begin();
path.addlines(-20,45, 20,45, 20,20, 10,20, 10,30, -10,30, -10,20, -20,20);
path.fill("#75aaff");

// inner ring
path.begin();
path.addlines(-10,-30, 10,-30, 10,30, -10,30);
path.close();
path.draw();

pen.width(@@penWidth);

#include "templates/construct-clip-ploygon-outline.spt"
#include "templates/construct-clip.spt"