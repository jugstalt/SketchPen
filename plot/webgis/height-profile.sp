brush.color("#fff");
gradientbrush.color("#fff");

path.begin();
path.addlines(-40,40, -40,-20, -20,-10, 0,-30, 20,-15, 40,-40, 40,40);
path.close();
path.fill();
path.draw();

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);

path.begin();
path.addlines(-40,40, -40,0, -20,5, 0,-10, 20,0, 40,-10, 40,40);
path.close();
path.fill();
path.draw();