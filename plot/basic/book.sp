path.begin();
path.addarc(270,90, 90,10, -45,45);
path.addarc(0,-90,  90,10, -45,-25);
path.close();
path.fill();
path.draw(@@outlinePenColor);

path.begin();
path.addarc(270,90, 70,25, -35,45);
path.addarc(0,-90,  70,25, -35,-25);
path.close();
path.fill();
path.draw();

// Lines

path.begin();
circle.arc(305,35, 70,25, -35,-10);
circle.arc(305,35, 70,25, -35,  3);
circle.arc(305,35, 70,25, -35, 16);
circle.arc(305,35, 70,25, -35, 30);


transform.scale(-1, 1);

path.begin();
path.addarc(270,90, 90,10, -45,45);
path.addarc(0,-90,  90,10, -45,-25);
path.close();
path.fill();
path.draw(@@outlinePenColor);

path.begin();
path.addarc(270,90, 70,25, -35,45);
path.addarc(0,-90,  70,25, -35,-25);
path.close();
path.fill();
path.draw();

path.begin();
path.addarc(270,90, 50,40, -25,45);
path.addarc(0,-90,  50,40, -25,-25);
path.close();
path.fill();
path.draw();