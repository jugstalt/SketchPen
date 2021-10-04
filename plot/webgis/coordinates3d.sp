#include "templates/axis3d.spt"

transform.reset();
transform.translate(35,35);
#include "templates/x.spt"

transform.reset();
transform.translate(-35,35);
#include "templates/y.spt"

transform.reset();
transform.translate(0,-35);
#include "templates/z.spt"