transform.reset();
path.begin();
path.addarc(180,90, 160,160, 40,40);

path.draw("#aaa");

path.begin();
path.addarc(0,360, 15,15, -40, 40);
path.fill("#aaa");

circle.draw(8,8, 40,-40, "#f00");
circle.draw(8,8, -20, -14, "#f00");
