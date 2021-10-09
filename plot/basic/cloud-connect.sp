line.draw(-35,-35,  35,35);
line.draw( 35,-35, -35,35);

path.begin():
path.addarc(0,360, 20,20, -35,35);
path.start();
path.addarc(0,360, 20,20,  35,35);
path.start():
path.addarc(0,360, 20,20, -35,-35);
path.start();
path.addarc(0,360, 20,20,  35,-35);
path.fill();
path.draw(@@outlinePenColor);


transform.reset();
transform.translate(0,-5);
transform.scale(0.7);
#include "templates/cloud.spt"
