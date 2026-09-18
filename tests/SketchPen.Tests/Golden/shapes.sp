// Every basic shape once: line, rect, circle, arc, pie, and a path made of
// straight lines, a cubic and a quadratic Bezier curve.
line.draw(-45, -45, -25, -45);

rect.fill(30, 20, 4);
rect.draw(30, 20, 4);

transform.translate(30, -30);
circle.fill(20);
circle.draw(20);
transform.reset();

circle.arc(0, 200, 30, 30, -25, 25);
circle.pie(0, 90, 20, 20, -25, 25);

path.begin();
path.addpoint(-40, 40);
path.addcubic(-30, 20, -10, 20, 0, 40);
path.addquad(10, 20, 20, 40);
path.addlines(30, 45, -40, 45);
path.close();
path.fill();
path.draw();
