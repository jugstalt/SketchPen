transform.reset();

line.draw(-40,45, -40,-45, "#ccc");
line.draw(-45,-40, 45,-20, "#ccc");

path.begin();
path.addarc(0,360, 15,15, -40, 40);
path.fill("#aaa");

circle.draw(8,8, 40,-22, "#f00");
circle.draw(8,8, -24,-36, "#f00");

path.begin();
path.addarc(180,90, 80,80, 0,10);
path.draw("#aaa");

