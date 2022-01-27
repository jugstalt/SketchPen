#include "templates/marker-full.spt"

path.draw(@@outlinePenColor);

//path.begin();
//path.addlines(-15,-10, 15,-10);
//path.close();
//path.addlines(0,-25, 0,5);
//path.close();

line.draw(-15,-10, 15,-10);
line.draw(0,-25, 0,5);

path.begin();
path.addlines(-10,-15, -15,-10, -10,-5);
path.close();
path.addlines( 10,-15,  15,-10,  10,-5);
path.close();
path.addlines(-5,-20, -0,-25, 5,-20);
path.close();
path.addlines(-5,0, -0,5, 5,0);
path.close();


path.draw();