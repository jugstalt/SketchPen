transform.reset();
path.begin();
path.addlines(-42,21, 0,-21, 42,21);

path.draw("#aaa");

path.begin();
path.addarc(0,360, 15,15, 0,-21);
path.fill("#f00");

path.begin();
path.addarc(0,360, 15,15, -42,21);
path.fill("#aaa");

path.begin();
path.addarc(0,360, 15,15, 42,21);
path.fill("#aaa");
