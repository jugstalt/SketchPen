// right
transform.reset();
transform.translate(10,0);

path.begin();
path.addlines(0,-30, 15,-30, 15,-15);
path.addarc(270,90, 30,30, 15,0);
path.addlines(30,0, 30,35);
path.addarc(0,-180, 30,15, 15,35);
path.close();
path.fill();
path.draw();

line.draw(3,-40, 12,-40);

path.begin();
path.addarc(180,-180, 30,15, 15,35);
path.draw();

// left
transform.reset();
transform.translate(-10,0);
transform.scale(-1,1);

path.begin();
path.addlines(0,-30, 15,-30, 15,-15);
path.addarc(270,90, 30,30, 15,0);
path.addlines(30,0, 30,35);
path.addarc(0,-180, 30,15, 15,35);
path.close();
path.fill();
path.draw();

line.draw(3,-40, 12,-40);

path.begin();
path.addarc(180,-180, 30,15, 15,35);
path.draw();

// connection
transform.reset();
line.draw(-10,-10, 10,-10);
line.draw(-10,0, 10,0);