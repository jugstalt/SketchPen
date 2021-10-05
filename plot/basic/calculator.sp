path.begin();
path.addlines(-35,45, -35,-45, 35,-45, 35,45);
path.close();
path.start();
path.addlines(-25,-35, 25,-35, 25,-25, -25,-25);
path.close();

path.fill();
path.draw();

brush.color(@@penColor);
gradientbrush.color(@@penColor);

// Line 1

path.begin();
path.addlines(-25,-15, -15,-15, -15,-5, -25,-5);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(-5,-15, 5,-15, 5,-5, -5,-5);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(15,-15, 25,-15, 25,-5, 15,-5);
path.close();
path.fill();
path.draw();

// Line 2

path.begin();
path.addlines(-25,5, -15,5, -15,15, -25,15);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(-5,5, 5,5, 5,15, -5,15);
path.close();
path.fill();
path.draw();

// Line 3

path.begin();
path.addlines(-25,25, -15,25, -15,35, -25,35);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(-5,25, 5,25, 5,35, -5,35);
path.close();
path.fill();
path.draw();

// Enter

path.begin();
path.addlines(15,5, 25,5, 25,35, 15,35);
path.close();
path.fill();
path.draw();