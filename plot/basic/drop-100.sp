path.begin();

path.addarc(-35,250, 70,70, 0,10);
path.addlines(0,-45, 0,-45);
path.close();

path.fill(@@penColor);
path.draw(@@outlinePenColor);