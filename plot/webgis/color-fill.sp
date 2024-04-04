brush.color("#ff0000");
gradientbrush.color("#aa0000");

path.begin();
path.addlines(-40,40, -40,-10, 0,-40, 40,-17, 30,40);
path.close();
path.fill();

path.draw("#ccc");

brush.color("#aaa");
gradientbrush.color("#aaa");

circle.fill(14,14,  -40,40);
circle.fill(14,14,  -40,-10);
circle.fill(14,14,  0,-40);
circle.fill(14,14,  40,-17);
circle.fill(14,14,  30,40);

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);