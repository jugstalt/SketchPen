// batch-print.sp

path.begin();
path.addlines(-25,-20, -25,-30, 35,-30, 35,22);
path.draw(@@outlinePenColor);

path.begin();
path.addlines(-15,-30, -15,-40, 45,-40, 45,12);
path.draw(@@outlinePenColor);

path.begin();
path.addlines(-35,30, -35,-20, 25,-20, 25,35);
path.addarc(0, 180, 20, 15, 35);
path.addlines(5,35, 5,30);
path.close();
path.fill();
path.draw(@@outlinePenColor);

path.begin();
path.addarc(90, 90, 20, 15, 35);
path.addlines( 5,35, 5,30, -45,30, -45,35);
path.addarc(180, -90,20 , -35,35);
path.close();
path.fill();
path.draw();

transform.reset();
transform.translate(-5, 5);
transform.scale(0.44);
#include "templates/arrow-arc.spt"
transform.reset();
transform.translate(-5, 5);
transform.scale(0.44);
transform.rotate(180);
#include "templates/arrow-arc.spt"

transform.reset();
line.draw(35, 30, 35,35);
line.draw(45, 20, 45,25);