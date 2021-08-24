#include "templates/zoom.spt"

// marker
brush.color(@@penColor);
path.start();
path.addarc(150,240, 30,30,0,-5);
path.addpoint(0, 22);
path.close();

path.start();
path.addarc(0, 360, 10,10, 0,-5);
path.close();

path.fill();
path.draw();