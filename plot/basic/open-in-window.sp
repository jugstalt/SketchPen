pen.width(@@penWidthBold);

line.draw(0, 0, 42, -42);
line.draw( 42, -42,  42,-25);
line.draw( 42, -42,  25,-42);

transform.reset();

path.begin();
path.addlines(42,-5, 42,42, -42,42, -42,-42, 5,-42);
path.draw();

pen.width(@@penWidth);