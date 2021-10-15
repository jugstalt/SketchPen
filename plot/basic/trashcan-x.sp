path.begin();
path.addlines(-32,-25, 32,-25, 27,42, -27,42);
path.close();
path.fill();
path.draw(@@outlinePenColor1);

path.begin();
path.addarc(180,180, 25,18, 0,-35);
path.draw();

line.draw(-35,-35, 35,-35, @@outlinePenColor);

transform.reset();
transform.translate( 0,10);
pen.width(@@penWidthBold);
line.draw(-15,-15,  15,15);
line.draw( 15,-15, -15,15);