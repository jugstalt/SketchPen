#include "templates/zoom.spt"

transform.reset();
transform.translate(-15,-15);

line.draw(-19,0, 19,0);
line.draw(0,-19, 0,19);


path.begin();
path.addlines(-19,0, -13,-6, -13,6);
path.fill(@@penColor);
path.close();
path.draw();

transform.rotate(90);
path.begin();
path.addlines(-19,0, -13,-6, -13,6);
path.fill(@@penColor);
path.close();
path.draw();

transform.rotate(180);
path.begin();
path.addlines(-19,0, -13,-6, -13,6);
path.fill(@@penColor);
path.close();
path.draw();

transform.rotate(270);
path.begin();
path.addlines(-19,0, -13,-6, -13,6);
path.fill(@@penColor);
path.close();
path.draw();