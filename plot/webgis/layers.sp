transform.translate(0,20);
path.begin();
#include "layer.spt"
path.fill();
path.draw();

transform.reset();
transform.translate(0,0);
path.begin();
#include "layer.spt"
path.fill();
path.draw();


transform.reset();
transform.translate(0,-20);
path.begin();
#include "layer.spt"
path.fill();
path.draw();