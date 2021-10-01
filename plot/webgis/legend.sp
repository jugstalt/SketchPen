transform.reset();
transform.translate(-30,-30);
path.begin();
path.addarc(0,360, 25,25, 0,0);
path.fill();
path.draw();

line.draw(25, 0, 70, 0);

transform.reset();
transform.translate(-30,0);
path.begin();
path.addlines(-12,-12, 12,-12, 12,12, -12,12);
path.close();
path.fill();
path.draw();

line.draw(25, 0, 70, 0);

transform.reset();
transform.translate(-30, 30);
path.begin();
path.addlines(0,-12, 12,12, -12,12);
path.close();
path.fill();
path.draw();

line.draw(25, 0, 70, 0);