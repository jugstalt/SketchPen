transform.reset();
transform.translate(0,-5);
transform.rotate(20);
path.begin();
path.addarc(260,200, 75,75, 0,0);
path.draw();
line.draw(0,-41, 0,41);

transform.scale(0.7);
#include "templates/globe2.spt"

transform.reset();
line.draw(0,35, 0,42);
line.draw(-20,42, 20,42);