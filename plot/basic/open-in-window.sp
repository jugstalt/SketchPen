pen.width(@@penWidthBold);

line.draw(0, 0, 42, -42);
line.draw( 42, -42,  42,-25);
line.draw( 42, -42,  25,-42);

pen.width(@@penWidth);

path.begin();
path.addlines(32,-5, 32,32, -32,32, -32,-32, 5,-32);
path.draw();

