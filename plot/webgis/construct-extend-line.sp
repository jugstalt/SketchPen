transform.reset();
transform.rotate(-15);
path.begin();
path.addlines(0,0, 45,0);
path.start();
path.addlines(33,-12, 45,0, 33,12);
path.draw();
circle.draw(6,6, 22,0);

transform.reset();
transform.rotate(140);
path.begin();
path.addlines(0,0, 45,0);
path.start();
path.addlines(33,-12, 45,0, 33,12);
path.draw();
circle.draw(6,6, 22,0);

transform.reset();
circle.draw(6,6, 0,0);