path.begin();
path.addlines(-42,42, -42,-42, 42,-42, 42,42);
path.close();
path.fill();
path.draw();

brush.color(@@penColor);
gradientbrush.color(@@penColor);

circle.fill(14,14,  -42,42);
circle.fill(14,14,  -42,-42);
circle.fill(14,14,  42,-42);
circle.fill(14,14,  42,42);

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);