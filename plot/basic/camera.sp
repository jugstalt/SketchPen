path.begin();
path.addlines(-45,-20, -20,-20, -15,-30, 15,-30, 20,-20, 45,-20);
path.addlines(45,30, -45,30);
path.close();

path.addarc(0,360, 38,38, 0,4);

path.fill();
path.draw();

circle.draw(8,8, 33,-9);
path.begin();
path.addarc(-90,270, 20,20, 0,4);
path.draw();