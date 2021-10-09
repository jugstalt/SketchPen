path.begin();
path.addarc(180,90, 42,42, -20,-20);
path.addarc(270,90, 42,42,  20,-20);
path.addarc(  0,90, 42,42,  20, 20);
path.addarc( 90,90, 42,42, -20, 20);
path.close();
path.fill();
path.draw(@@outlinePenColor);

brush.color("#fefefe");
gradientbrush.color("#fefefe");

path.begin();
path.addlines(-15,-15, 15,-15, 15,15, -15,15);
path.close();
path.fill();
path.draw();


transform.reset();
transform.rotate(-45);

line.draw(20,0, 42,0);
line.draw(42,0, 35,-7);
line.draw(42,0, 35, 7);