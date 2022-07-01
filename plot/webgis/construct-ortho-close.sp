transform.reset();
transform.translate(-5,10);
transform.rotate(-75);

path.begin();
path.addlines(-20,38, -20,10, 5,10, 5,-38, , 38,-38);
path.draw();

path.begin();
path.addlines(38,-38, 38,38, -20,38);
path.draw("#aaa");

circle.draw(8,8, -20, 38);
circle.draw(8,8, -20, 10);
circle.draw(8,8,  5,10);
circle.draw(8,8, 5,-38);
circle.draw(8,8, 38,-38);

circle.draw(8,8, 38,38, "#f00");