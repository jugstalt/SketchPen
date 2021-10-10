//transform.reset();
//transform.translate(-10,-10);
//transform.scale(0.8);
#include "templates/page.spt"

line.draw(-20,-30, -10,-30);
line.draw(-20,-15, -10,-15);
line.draw(-20,  0,  20,  0);
line.draw(-20, 15,  20, 15);
line.draw(-20, 30,  20, 30);

transform.reset();
transform.translate(15, -2);
transform.rotate(-45);
transform.scale(0.9);

#include "templates/pen-halo.spt"
#include "templates/pen.spt"