transform.reset();
transform.rotate(-45);

path.begin();
path.addlines(-10,-15, 45,-15, 45,15, -10,15);
path.close();
path.fill();
path.draw();

path.begin();
path.addlines(-10,-13, -20,-13, -34,-6, -34,6, -20,13, -10,13);
path.draw();

path.begin();
path.addlines(-34,-4, -44,-4, -41,4, -34,4);
path.draw();

transform.reset();

line.draw(-45,40, 45,40);