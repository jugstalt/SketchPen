// bow
pen.width(4);
pen.color("#aaa");
circle.draw( 90,120, 0,50);
pen.width(@@penWidth);
pen.color(@@penColor);


// sun
circle.fill(32,32, 0,-10);
circle.draw(32,32, 0,-10);

// rays
transform.reset();
transform.translate(0, -10);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.rotate(45);
line.draw(0,-25, 0,-33);
transform.reset();

//terrain
path.begin();
path.addlines(-45,50, -25,25, -15,45, 0,30, 15,45, 25,25, 45,50);

path.fill();
path.draw();