transform.reset();
transform.rotate(-45);
#include "templates/pen-redlining.spt"

transform.reset();

path.begin();
path.addarc(270,-185, 20,80, -35,0);

path.draw();