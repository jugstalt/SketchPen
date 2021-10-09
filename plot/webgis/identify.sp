path.begin();
path.addarc(0,360, 80,80, 0,0);
path.fill();
path.draw(@@outlinePenColor);

pen.width(@@penWidthBold);
line.draw(0,-5, 0,20);
line.draw(0,-20, 0,-21);