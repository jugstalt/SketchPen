path.begin();
path.addlines(0,-45, 45,-25, 0,-5, -45,-25);
path.close();

path.start();
path.addlines(-5,2.3, -45,-15, -45,25, -5,45);
path.close();

path.start();
path.addlines(5,2.3, 45,-15, 45,25, 5,45);
path.close();

path.fill();
path.draw(@@outlinePenColor);

path.begin();
path.addlines( 45,-25, 0,-5, -45,-25);

path.start();
path.addlines(-45,-15, -5,2.3, -5,45);

path.start();
path.addlines(45,-15, 5,2.3, 5,45);

path.draw();