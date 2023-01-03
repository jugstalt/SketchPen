// construct-copy.sp

transform.reset();
transform.translate(-20,-20);

path.begin();
path.addlines(-13,-20, 17,-20, 17,20, -17,20, -17,-15);
path.close();
path.fill();
path.draw();


transform.reset();
transform.translate(20,20);

path.begin();
path.addlines(-13,-20, 17,-20, 17,20, -17,20, -17,-15);
path.close();
path.fill();
path.draw();

transform.reset();
transform.translate(5,-10);

path.begin();
path.addarc(270,90, 30,50, 0,0);
path.start();
path.addlines(5,-10, 15,0, 25,-10);
path.draw();
