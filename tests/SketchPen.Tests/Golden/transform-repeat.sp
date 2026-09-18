// A radial pattern via repeat + cumulative rotation, then a scaled copy
// and a rotation around a pivot.
repeat(8) {
    line.draw(0, -20, 0, -40);
    transform.rotate(45);
}

transform.reset();
transform.scale(0.4);
circle.fill(60);
circle.draw(60);

transform.reset();
transform.rotate(30, 20, 20);
rect.draw(10, 10);
