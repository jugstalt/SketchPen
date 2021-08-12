pen.width(8);
pen.cap("flat")
line.draw(15,-30, 15,0);

pen.width(@@penWidth);
pen.cap(@@penCap);
path.start();
path.addlines(-30,30, -10,30, -10,5, 10,5, 10, 30, 30, 30);
path.addlines(30, 30, 30,-10, 0,-30, -30,-10, -30, 30);

path.fill();
path.draw();
