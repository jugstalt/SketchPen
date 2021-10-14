transform.reset();
transform.translate(0,30);
path.begin();
path.addlines(-45,0, -25,-15, 25,15, 45,0);
path.draw();

transform.reset();
transform.translate(0,-30);
path.begin();
path.addlines(-45,0, -25,-15, 25,15, 45,0);
path.draw();

transform.reset();
transform.rotate(135);
transform.translate(2,10);
transform.scale(0.6);
#include "../basic/templates/arrow-small.spt"