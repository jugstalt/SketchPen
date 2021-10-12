#include "templates/round.spt"

path.begin();
path.addarc(210,120, 60,60, 0,40);
path.start();
path.addarc(0,360, 35,35, 0,-7.5);
path.draw();