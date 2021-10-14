transform.reset();
transform.rotate(-30);

path.begin();
path.addlines(-45,0, 45,0);
path.draw();

circle.draw(10,10, -45,0);
circle.draw(10,10,  45,0);

transform.rotate(45);

path.begin();
path.addlines(-10,-10, 10,-10, 10,10, -10,10);
path.close();
path.fill();
path.draw(@@outlinePenColor);

