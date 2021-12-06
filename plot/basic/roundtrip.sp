transform.reset();
transform.rotate(45);
transform.translate(-35, 0);
transform.scale(0.3);

#include "templates/plane.spt"

transform.reset();
transform.rotate(225);
transform.translate(-35, 0);
transform.scale(0.3);

#include "templates/plane.spt"

transform.reset();
transform.rotate(45);

circle.draw(10,10, 0,-45);
circle.draw(10,10, 0, 45);

path.begin();
path.addarc(-70,35, 70,90, 0,0);
path.draw();

path.begin();
path.addarc(35,35, 70,90, 0,0);
path.draw();

path.begin();
path.addarc(110,35, 70,90, 0,0);
path.draw();

path.begin();
path.addarc(215,35, 70,90, 0,0);
path.draw();

