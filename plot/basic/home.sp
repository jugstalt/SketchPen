pen.width(12);
pen.cap("flat")
line.draw(25,-42, 25,0);

pen.width(@@penWidth);
pen.cap(@@penCap);
path.start();
path.addlines(-42,42, -15,42, -15,5, 15,5, 15,42, 42,42);
path.addlines(42,42, 42,-10, 0,-42, -42,-10, -42, 42);

path.fill();
path.draw(@@outlinePenColor);

path.begin();
path.addlines(45,-7.6, 0,-42, -45,-7.6);
path.draw();

line.draw(-45,42, 45,42);