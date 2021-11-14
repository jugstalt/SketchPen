path.begin();
path.addlines(0,0, 45,0);
path.addarc(0,-270, 90,90, 0,0);
path.close();

path.fill();
path.draw(@@outlinePenColor);

circle.draw(90,90, 0,0);