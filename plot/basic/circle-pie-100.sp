path.begin();
path.addarc(0,-360, 90,90, 0,0);
path.close();

path.fill();
path.draw(@@outlinePenColor);

circle.draw(90,90, 0,0);