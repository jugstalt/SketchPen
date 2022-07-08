transform.reset();
path.begin();
path.addlines(-40,-30, 40,-30, 40,30, -40,30);
path.close();

path.draw("#aaa");

path.begin();
path.addarc(0,360, 15,15, -40,-30);
path.fill("#aaa");

circle.draw(8,8,  40,-30, "#f00");
circle.draw(8,8,  40, 30, "#f00");
circle.draw(8,8, -40, 30, "#f00");
