transform.reset();
transform.rotate(45);

// fins
path.begin();
path.addlines(0,10, 22,30, 21,45, 12,40, -12,40, -21,45, -22,30);
path.close();
path.fill("#fefefe");
path.draw();

// fuel
path.begin();
path.addlines(0,66, -12,50, -10,40);
path.addarc(180,-180, 20,20, 0,40);
path.addlines(12,50, 0,66);
path.fill("#fa0");
//path.draw();

// body
path.begin();
path.addlines(0,-50, 20,-27, 12,40, -12,40, -20,-27);
path.close();
path.start();
path.addarc(0,360, 10,10, 0,-23);

path.fill();
path.draw(@@outlinePenColor);

// pull eye
path.begin();
path.addarc(0,360, 10,10, 0,-23);
path.draw();