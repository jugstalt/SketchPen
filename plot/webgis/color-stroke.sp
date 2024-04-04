brush.color("#aa0000");
gradientbrush.color("#aa0000");
pen.color("#ff0000");

path.begin();
path.addlines(-40,40, -20,20, 5,23, -5,-23, 20,-20, 40,-40);
path.draw();

circle.fill(14,14,  -40,40);
circle.fill(14,14,  -20,20);
circle.fill(14,14,  5,23);
circle.fill(14,14,  -5,-23);
circle.fill(14,14,  20,-20);
circle.fill(14,14,  40,-40);

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);
pen.color(@@penColor);