brush.color(@@penColor);
gradientbrush.color(@@penColor);

//transform.scale(0.1);
transform.translate(0,-10);

path.begin();
path.addlines(-45,40, -45,-24);
path.addarc(180,90, 10,10, -40,-24);
path.addlines(-40,-29, -10, -29);
path.addarc(270,90, 10,10, -10,-24);
path.addlines(-5,-24, -5,-20, 20,-20);
path.addarc(270,90, 10,10, 25,-15);
path.addlines(30,-15, 30,40);
path.addarc(0,90, 10,10, 25,40);
path.addlines(20,45, -40,45);
path.addarc(90,90, 10,10, -40,40);

path.fill();
path.draw();

brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);

path.begin();
path.addlines(-45,45, -30,-10);
path.addlines(-30,-10, 45,-10);
path.addlines(45,-10, 30,45);
path.addlines(30,45, -45,45);

path.fill();
path.draw();