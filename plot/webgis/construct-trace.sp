path.begin();
path.addlines(-40,40,-40,-10, 20,-10, 20,-40);
path.start();
path.addlines(10,-30,  20,-40, 30,-30)
path.draw();

path.begin();
path.addlines(-20,40, -20,10, 40,10, 40,-40);
path.draw();

circle.draw(8,8, -20, 40);
circle.draw(8,8, -20, 10);
circle.draw(8,8,  40, 10);
circle.draw(8,8, 40,-40);