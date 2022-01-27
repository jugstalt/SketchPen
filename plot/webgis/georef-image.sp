// globe

//brush.color("#fff");
//gradientbrush.color("#fff");

// #include "./../basic/templates/globe2.spt"

// brush.color(@@brushColor);
// gradientbrush.color(@@gradientbrushColor);

// image
transform.reset();
transform.translate(0,10);
transform.rotate(20);
path.begin();
path.addlines(-40,-25, 40,-25, 40,25, -40,25);
path.close():
path.fill();
path.draw(@@outlinePenColor);

// marker
transform.reset();
transform.translate(10,-10);
transform.rotate(45);
transform.scale(0.8);
#include "./../basic/templates/pin.spt"