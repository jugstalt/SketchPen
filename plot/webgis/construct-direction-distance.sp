transform.reset();
path.begin();
path.addlines(-45,45, 30,-45);
path.start();
path.addarc(0,360, 70,70, 0,0);
path.draw("#aaa");

path.begin();
path.addarc(0,360, 15,15, 17,-29);
path.fill("#f00");

path.begin();
path.addarc(0,360, 15,15, -25,24);
path.fill("#f00");
