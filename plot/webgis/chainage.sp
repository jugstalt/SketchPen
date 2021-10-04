// polyline
path.begin();
path.addlines(-40,40, -25,10, 25,-10, 40,-40);
path.draw();

transform.reset();
transform.scale(0.6, 0.6);
#include "templates/altitude-marker.spt"

transform.reset();
transform.translate(30,30);
#include "templates/x.spt"