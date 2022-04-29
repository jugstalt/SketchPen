
path.begin();
path.addlines(-20,-45, 20,-45, 20,-20, -20,-20);
path.close();
path.fill("#0061f6");

path.begin();
path.addlines(-20,45, 20,45, 20,20, -20,20);
path.close();
path.fill("#0061f6");

#include "templates/construct-clip-ploygon-outline.spt"
#include "templates/construct-clip.spt"