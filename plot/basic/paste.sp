// paste.sp

path.begin();
path.addlines(-8, 36, -30, 36);
path.addarc(90, 90, 12, -36, 30);
path.addlines(-42, 30, -42,-30);
path.addarc(180, 90, 12, -36, -30);
path.addlines(-36, -36, -23, -36);
path.draw();

path.begin();
path.addlines(7,-28, -23,-28, -23, -39);
path.addarc(180, 90, 10, -18, -39);
path.addlines(-18,-44, 2,-44);
path.addarc(270, 90, 10, 2, -39);
path.close();
path.draw();

path.begin();
path.addlines(7, -36, 10, -36);
path.addarc(270, 90, 12, 21, -30);
path.draw();

transform.reset();
transform.translate(25,11);
#include "templates/page-small.spt"