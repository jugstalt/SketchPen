// Like (thumbs up, hand seen from the side)
transform.reset();

// wrist / forearm stub on the left, drawn first so the fist overlaps it
transform.translate(-18, 21);
rect.fill(36, 22, 6, @@brushColor);
rect.draw(36, 22, 6, @@outlinePenColor);
transform.reset();

// fist with raised thumb
path.begin();
path.addpoint(-8, 32);
path.addlines(-8, 32, -8, -20);
path.addcubic(-8, -30, -3, -35, 3, -35);
path.addcubic(9, -35, 12, -30, 12, -21);
path.addlines(12, -21, 12, -15);
path.addcubic(16, -20, 20, -21, 24, -20);
path.addcubic(28, -19, 30, -15, 30, -10);
path.addlines(30, -10, 30, 25);
path.addcubic(30, 30, 27, 32, 22, 32);
path.close();
path.fill(@@brushColor);
path.draw(@@outlinePenColor);

// curled fingers (short folds on the knuckle side)
line.draw(4, 1, 30, 1, @@outlinePenColor);
line.draw(4, 12, 30, 12, @@outlinePenColor);
line.draw(4, 23, 30, 23, @@outlinePenColor);
