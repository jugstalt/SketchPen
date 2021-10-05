path.begin();
path.addlines(-42,-42, 42,-42, 42,30, -42,30);
path.close();
path.draw();

path.begin();
path.addlines(-42,-42, 42,-42, 42,14, -42,14);
path.close();
path.fill();
path.draw();

line.draw(30, 22, 33,22);

line.draw(-20,42, 20,42);
line.draw(-15,42, -10,30);
line.draw( 15,42,  10,30);