path.begin();
path.addarc(0,360, 84,84, 0,0);
path.fill();
path.draw(@@outlinePenColor);

pen.width(10);
path.begin();
path.addlines(0,40, 0,-8);
path.addarc(180,90, 30,25, 15,-8);
path.start();
path.addlines(-10,1, 10,1);
path.draw();