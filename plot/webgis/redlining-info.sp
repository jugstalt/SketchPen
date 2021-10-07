transform.reset();
transform.translate(-15,-15);
transform.scale(0.7);
#include "../basic/templates/table.spt"

transform.reset();
transform.translate(20,15);
transform.rotate(-45);
transform.scale(0.6);

#include "templates/pen-redlining.spt"

transform.reset();
line.draw(-45,40, 45,40);