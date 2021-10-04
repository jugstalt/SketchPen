// doc
path.begin();
path.addlines(-45,-10, 0,-10, 0,45, -45,45);
path.close();
path.fill();
path.draw();

line.draw(-35,3.75, -10,3.75);
line.draw(-35,17.5, -10,17.5);
line.draw(-35,31.25, -10,31.25);

// arrow
path.begin();
path.addlines(8,35, 25,35, 25,13);
path.start();
path.addlines(20,20, 25,13, 30,20);
path.draw();

// marker
transform.reset();
transform.translate(25,-20);
transform.scale(0.6);
#include "templates/marker-filled.spt"