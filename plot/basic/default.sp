#include "templates/rack-wheel.spt"

circle.fill(42,42, 0,0, @@backgroundBrushColor);
circle.draw(42);

path.begin();
path.addlines(-10,0, -3,7, 7,-7);
path.draw();