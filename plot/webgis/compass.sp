path.begin();
path.addarc(0,360, 90,90);
path.fill();
path.draw();

line.draw(-45,0, -35,0);
line.draw( 45,0,  35,0);
line.draw(0,-45, 0,-35);
line.draw(0, 45, 0, 35);

transform.reset();
transform.rotate(45);

brush.color("#000");
gradientbrush.color("#000");
path.begin();
path.addlines(0,-30, 10,0, -10,0);
path.close();
path.fill();
path.draw();

brush.color("#fff");
gradientbrush.color("#fff");
path.begin();
path.addlines(0, 30, 10,0, -10,0);
path.close();
path.fill();
path.draw();

circle.fill(15,15, -54,0);
circle.draw(15,15, -54,0);

