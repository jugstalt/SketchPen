
// Paper input
path.begin();
path.addlines(-25,-19, -25,-40, 25,-40, 25,-19);
path.close();

path.fill("#fff");
path.draw();

// Printer
path.begin();

path.addarc(180,90, 18,18, -35,-10);
path.addarc(270,90, 18,18, 35,-10);

path.addlines(44,18, -44,18);
path.close();

path.fill();
path.draw(@@outlinePenColor);

// Power
path.begin();
path.addarc(0,360, 7,7, -35,-10);
path.draw();

// Paper output
path.begin();

path.addlines(-25,30, -25,3, 25,3, 25,30);
path.close();

path.fill("#fff");
path.draw();