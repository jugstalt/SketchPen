transform.reset();
path.begin();
path.addarc(0,360, 50,50, 20,20);
path.start();
path.addarc(0,360, 70,70, -10,-10);
path.draw("#aaa");

path.begin();
path.addarc(0,360, 15,15, 23,-6);
path.fill("#f00");

path.begin();
path.addarc(0,360, 15,15, -6,23);
path.fill("#f00");
