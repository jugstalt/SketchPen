transform.reset();
transform.translate(-10, -10);
transform.scale(0.8);

circle.arc(200,140, 80,80, 0,50)

circle.fill(50,50, 0,-15);
circle.draw(50,50, 0,-15, @@outlinePenColor);

transform.reset();
transform.translate(21, 21);
transform.scale(0.6);
transform.rotate(22);
#include "templates/rack-wheel.spt"