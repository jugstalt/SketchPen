transform.reset();
transform.rotate(-30);


line.draw(-45,0, 45,0);
circle.draw(10,10, -45,0);
circle.draw(10,10,  45,0);

line.draw(-10,-40, 10,40);
circle.draw(10,10, -10,-40);
circle.draw(10,10,  10, 40);

transform.rotate(45);

path.begin();
path.addlines(-10,-10, 10,-10, 10,10, -10,10);
path.close();
path.fill();
path.draw(@@outlinePenColor);