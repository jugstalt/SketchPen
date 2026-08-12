// path.addcubic / path.addquad  (new)
// -------------------------------------
// Cubic and quadratic Bezier curve segments, continuing from the path
// current point -- both REQUIRE a current point already established via a
// prior addpoint/addlines/addarc (they have nothing to curve "from"
// otherwise, and will throw a clear error when there is none).
//
// addcubic(cp1x,cp1y, cp2x,cp2y, x,y)  -- two control points + end point
// addquad(cpx,cpy, x,y)                -- one control point + end point

transform.reset();
transform.translate(-22, 0);
path.begin();
path.addpoint(-18, 10);
path.addcubic(-9,-24, 9,-24, 18,10);   // cp1, cp2, end
path.draw(@@accentColor, 5);

transform.reset();
transform.translate(22, 0);
path.begin();
path.addpoint(-18, 10);
path.addquad(0, -24, 18, 10);          // cp, end
path.draw(@@accentColor, 5);

transform.reset();
