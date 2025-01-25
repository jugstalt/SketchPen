// openid.sp

brush.color(@@brushColor);
circle.fill(85);
circle.draw(85);

path.begin();
path.addarc(90, 220, 50, 30, 0, 10);
path.draw(@@outlinePenColor);


transform.translate(20, 0);
transform.rotate(20);

path.begin();
path.addlines(-5,-5, 0,0, -5,5);
path.draw(@@outlinePenColor);

transform.reset();
path.begin();
path.addlines(0,25, 0,-25);
path.draw();