path.begin();
path.addlines(-45,40, -45,-40);
path.addarc(180,90, 10,10, -40,-40);
path.addlines(-40,-45, 27,-45);
path.addlines(45,-27, 45,40);
path.addarc(0,90, 10,10, 40,40);
path.addlines(40,45, -40,45);
path.addarc(90,90, 10,10, -40,40);
path.close();
path.fill(@@backgroundBrushColor);
path.draw();


// label
path.begin();
path.addlines(-35,35, -35,0);
path.addlines(-35,0, 35,0);
path.addlines(35,0, 35,35);
path.addlines(35,35, -35,35);
path.fill();
path.draw(@@outlinePenColor);

// hole
path.begin();
path.addlines(-20,-42, 20,-42, 20,-18, -20,-18);
path.close();
path.start();
path.addlines(2,-38, 12,-38, 12,-22, 2,-22);
path.close();
path.fill();
path.draw(@@outlinePenColor);
