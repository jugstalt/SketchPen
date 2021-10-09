path.begin();
path.addlines(30,-25, -30,0, 30,25);
path.draw();

circle.fill(26, 30,-25);
circle.draw(26,26, 30,-25, @@outlinePenColor);

circle.fill(26, -30,0);
circle.draw(26,26, -30,0, @@outlinePenColor);

circle.fill(26, 30,25);
circle.draw(26,26, 30,25, @@outlinePenColor);