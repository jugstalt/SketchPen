// circle
// -------
// Full circle, circular arc (outline only), or pie segment (filled). The
// `pos...` parameters (1-4 numbers) always describe a rectangle by center +
// diameter -- see docs/SYNTAX.md > "Position and point parameters" for the
// exact rules (1 = diameter, 2 = diameterX,diameterY, 3 = diameter,x,y,
// 4 = diameterX,diameterY,x,y). Angles: 0 degrees points right (+X),
// positive angles rotate clockwise (Y grows downward).

transform.reset();
transform.translate(-22, -22);
circle.fill(28);                       // full circle, diameter 28, centered here

transform.reset();
transform.translate(22, -22);
circle.draw(28, 28, 0, 0, "#e0592a");  // outline only, explicit center + color

transform.reset();
transform.translate(-22, 22);
circle.arc(200, 140, 28, 28, 0, 0);    // arc: from 200 degrees, sweeping 140 degrees

transform.reset();
transform.translate(22, 22);
circle.pie(0, -270, 28, 28, 0, 0);     // filled pie slice (a "75%" wedge)

transform.reset();
