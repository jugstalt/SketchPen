
transform.reset();
transform.translate(0,31);
path.begin();
path.addlines(-45,0, 45,0);
path.start();
path.addarc(0,-45, 80,80, -45,0);

path.draw();

transform.reset();
transform.rotate(135);
transform.translate(10,10);
#include "../basic/templates/arrow.spt"


