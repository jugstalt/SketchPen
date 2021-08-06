transform.rotate(180);

// Big Circle
pen.color("#82C828");
pen.width(8);
brush.color("#fff");
gradientbrush.color("#b5dbad");

path.start();
path.addlines( 0, -30, 42, 30,-42, 30, 0, -30);
path.fill();
path.draw();