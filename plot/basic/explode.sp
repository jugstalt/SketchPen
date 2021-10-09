path.begin();

path.addarc(0,360, 40,40, -25,0);
path.addarc(0,360, 40,40, 25,-25);
path.addarc(0,360, 40,40, 25, 25);

path.fill();
path.draw(@@outlinePenColor);

transform.reset();
transform.translate(-25,0);
transform.rotate(-28);
line.draw( 0,0, 60,0);
line.draw(60,0, 50,-10);
line.draw(60,0, 50, 10);

transform.reset();
transform.translate(-25,0);
transform.rotate(28);
line.draw( 0,0, 60,0);
line.draw(60,0, 50,-10);
line.draw(60,0, 50, 10);