#include "drop-0.sp"

path.begin();
path.addarc(0,180, 70,70, 0,10);
path.close();

path.fill(@@penColor);
path.draw();