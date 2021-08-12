brush.color("#fefefe");
gradientbrush.color("#fefefe");

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

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);

transform.reset();
transform.translate(0,-20);
path.begin();
#include "layer.spt"
path.fill();
path.draw();