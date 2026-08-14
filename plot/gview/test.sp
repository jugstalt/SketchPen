// Cybernetic skull portrait
transform.reset();

// Angular metal cranium
path.begin();
path.addlines(-24, -40, 24, -40, 31, -27, 29, 15, 20, 34, -20, 34, -29, 15, -31, -27);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

// Side plates
path.begin();
path.addlines(-31, -20, -37, -14, -35, 13, -29, 18);
path.draw(@@outlinePenColor);
path.begin();
path.addlines(31, -20, 37, -14, 35, 13, 29, 18);
path.draw(@@outlinePenColor);

// Brow ridge
path.begin();
path.addlines(-25, -12, -5, -5, 0, -9, 5, -5, 25, -12, 20, -3, 5, -1, 0, 3, -5, -1, -20, -3);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);

// Eye sockets and red optical lenses
path.begin();
path.addlines(-23, -4, -6, -10, -8, -1, -22, 4);
path.close();
path.fill(@@backgroundBrushColor);
path.begin();
path.addlines(23, -4, 6, -10, 8, -1, 22, 4);
path.close();
path.fill(@@backgroundBrushColor);
circle.fill(5, -14, -3, @@penColor);
circle.fill(5, 14, -3, @@penColor);

// Nose bridge and cheek plates
path.begin();
path.addlines(-4, -8, 4, -8, 7, 12, 0, 18, -7, 12);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);
path.begin();
path.addlines(-25, 7, -10, 12, -12, 27, -22, 22);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);
path.begin();
path.addlines(25, 7, 10, 12, 12, 27, 22, 22);
path.close();
path.fill(@@gradientbrushColor);
path.draw(@@outlinePenColor);

// Exposed metal jaw and teeth
path.begin();
path.addlines(-16, 20, -10, 23, -6, 19, -2, 23, 2, 19, 6, 23, 10, 20, 16, 20, 12, 34, -12, 34);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);
line.draw(-10, 22, -10, 32, @@outlinePenColor);
line.draw(-3, 22, -3, 33, @@outlinePenColor);
line.draw(3, 22, 3, 33, @@outlinePenColor);
line.draw(10, 22, 10, 32, @@outlinePenColor);

// Central jaw seam
line.draw(0, 18, 0, 34, @@outlinePenColor);

// Facial damage lines
line.draw(-26, 4, -18, 15, @@outlinePenColor);
line.draw(26, 4, 18, 15, @@outlinePenColor);
