transform.reset();
transform.scale(0.75, 0.5);
transform.rotate(45);

path.begin();
path.addlines(-45,-45, -23,-45, -23,-23, -45,-23);
path.close();
path.addlines(-11,-45, 11,-45, 11,-23, -11,-23);
path.close();
path.addlines(23,-45, 45,-45, 45,-23, 23,-23);
path.close();

path.addlines(-45,-11, -23,-11, -23,11, -45,11);
path.close();
path.addlines(-11,-11, 11,-11, 11,11, -11,11);
path.close();
path.addlines(23,-11, 45,-11, 45,11, 23,11);
path.close();

path.addlines(-45,23, -23,23, -23,45, -45,45);
path.close();
path.addlines(-11,23, 11,23, 11,45, -11,45);
path.close();
path.addlines(23,23, 45,23, 45,45, 23,45);
path.close();

path.fill();
path.draw();