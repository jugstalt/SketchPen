// axis-geographic-minus-90.sp
#include "templates/axis.spt"

text.draw( "90", 15 , 25,  5, "#000");
text.draw(  "0", 15 , -4,-25, "#000");
text.draw("270", 15 ,-50,  5, "#000");
text.draw("180", 15 , -12, 35, "#000");

pen.width(1);

transform.rotate(-90);
circle.arc(0, 90, 20);
line.draw(0, 10, 3, 12);
line.draw(0, 10, 3,  7);