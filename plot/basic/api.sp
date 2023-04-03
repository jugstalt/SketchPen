// api.sp

#include "templates/rack-wheel-full.spt"

transform.scale(0.75);

// A
transform.translate(-17, 0);
path.begin();
path.addlines(-11,15, 0,-15, 11,15);
path.start();
path.addlines(-7,5, 7,5);
path.draw();

// P 
transform.translate(25, 0);
path.begin();
path.addlines(-7,15, -7,-15);
path.addarc(-90, 180, 15,15, 3, -8);
path.addpoint(-7, 0);
path.draw();

// I
transform.translate(18, 0);
path.begin();
path.addlines(0,15, 0,-15);
path.draw();