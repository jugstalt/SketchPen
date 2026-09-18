// Variables and arithmetic: @@percent and @@x come from styles/default.globals.
path.begin();
path.addlines(0, 0, 30, 0);
path.addarc(0, -3.6 * @@percent, 60, 60, 0, 0);
path.close();
path.fill();
path.draw();

circle.draw(@@x - 10, 20 + 2 * 5, -20, 30);
