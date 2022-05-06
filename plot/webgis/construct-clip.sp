pen.width(4);
path.begin();
path.addlines(-40,-30, 0,-45, 45,-20, 40,20, 10,45, -45,20);
path.close();
path.draw("#aaa");
pen.width(@@penWidth);


transform.reset();
transform.rotate(30);
transform.scale(0.8);
circle.draw(30,20, -30,0);
line.draw(-15,0, 42, 0);

transform.reset();
transform.rotate(-30);
transform.scale(0.8);
circle.draw(30,20, -30,0);
line.draw(-15,0, 42, 0);