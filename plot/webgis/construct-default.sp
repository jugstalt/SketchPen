transform.reset();

path.begin();
path.addlines(-40,20, -20,-10, 10,30);
path.draw();


path.begin();
path.addlines(10,30, 40,-30);
path.draw("#aaa");

circle.draw(8,8, -40, 20);
circle.draw(8,8, -20,-10);
circle.draw(8,8,  10, 30);
circle.draw(8,8,  40,-30, "#aaa");