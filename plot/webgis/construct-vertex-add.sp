path.begin();
path.addlines(-45,45, -30,-30, 45,-45);
path.draw();

transform.rotate(45);
transform.translate(-35,0);
path.begin();
path.addlines(-10,-10, 10,-10, 10,10, -10,10);
path.close();
path.fill();
path.draw(@@outlinePenColor);

transform.reset();
transform.translate(15,15);
transform.scale(0.75);
#include "../basic/templates/round-plus.spt"