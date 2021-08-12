#include "position.spt"

transform.rotate(30);

pen.width(4);
path.start();
path.addlines(0, -15, 9,15, 0,10, -9,15, 0,-15);
path.fill();
path.draw();