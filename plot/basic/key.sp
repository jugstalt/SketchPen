// key.sp

transform.reset();
transform.rotate(-45);

path.begin();
path.addarc(0,360, 30,30,30,0);
path.fill();
path.draw();

path.begin();
path.addlines(-40,0, 12,0);
path.draw();

path.begin();
path.addlines(-36,10, -36,0);
path.draw();

path.begin();
path.addlines(-28,7, -28,0);
path.draw();

path.begin();
path.addlines(-20,10, -20,0);
path.draw();