pen.width(12);
pen.cap("flat")
line.draw(25,-45, 25,0);

pen.width(@@penWidth);
pen.cap(@@penCap);
path.start();
path.addlines(-45,45, -15,45, -15,5, 15,5, 15,45, 45,45);
path.addlines(45,45, 45,-10, 0,-45, -45,-10, -45, 45);

path.fill();
path.draw();
