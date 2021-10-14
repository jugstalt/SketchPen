transform.reset();
transform.rotate(45);
path.begin();
path.addlines(-50,-15, -20,-15, -20,15, -50,15);
path.close();
path.addlines( 50,-15,  20,-15,  20,15,  50,15);
path.close();

path.fill();
path.draw(@@outlinePenColor);

transform.rotate(180);
transform.scale(0.6);
#include "../basic/templates/arrow-small.spt"
