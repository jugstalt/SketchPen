// package.sp

path.begin();
path.addlines(0,0, -35,-10, -35,35, 0,45);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(-35,-10, 0,-20, 35,-10);
path.draw();

path.begin();
path.addlines(-35,-10, -45,-25, -10,-35, 0,-20);
path.draw();

path.begin();
path.addlines(0,-20, 10,-35, 45,-25, 35,-10);
path.draw();

path.begin();
path.addlines(35,-10, 35,35, 0,45, 0,0);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(0,0, 10,-15, 45,-25, 35,-10);
path.close();
path.fill();
path.draw();