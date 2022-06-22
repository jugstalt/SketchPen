transform.reset();
path.begin();
path.addlines(-45,45, 30,-45);
path.start();
path.addlines(45,45, -45,-45);

path.draw("#aaa");

path.begin();
path.addarc(0,360, 15,15, -4,-4);
path.fill("#f00");
