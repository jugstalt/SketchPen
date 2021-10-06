path.begin();
path.addarc(52,37, 100,100, -45,-17);
path.addarc(120,-125, 100,100, -20,-10);
path.addlines(45,-20, 30,-21, 42,-30);
path.addarc(330,-150, 30,30, 12,-20);
path.addarc(90,30, 150,150, -5,-95);

path.addarc(150,-30, 80,80, -7,-45);
path.addarc(150,-30, 80,80, -7,-30);
path.addarc(130,-30, 80,80, -10,-20);
path.close();

path.fill();
path.draw();