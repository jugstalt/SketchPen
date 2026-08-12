// transform
// ----------
// Cumulative coordinate-system changes (translate/rotate/scale) applied to
// every subsequent drawing command, until `transform.reset()` restores the
// original state (origin at the canvas center, no rotation/scale).
//
// `rotate` has two forms: the classic `rotate(angle)` (around the canvas
// origin) and the pivot overload added this session, `rotate(angle,
// pivotX, pivotY)` (around an arbitrary point instead) -- both shown below,
// so the difference is visible: the left rectangle tilts around the whole
// canvas center, the right one tilts around its own corner instead.

// classic: rotate around the canvas origin
transform.reset();
transform.translate(-22, 0);
transform.rotate(20);
rect.fill(30, 18, 0, "#cfe3f7");
rect.draw(30, 18);

// pivot overload: rotate around an arbitrary point -- equivalent to
// transform.translate(px,py); transform.rotate(angle); transform.translate(-px,-py);
// but as one call
transform.reset();
transform.translate(22, 0);
transform.rotate(20, -15, -9);
rect.fill(30, 18, 0, "#f7d774");
rect.draw(30, 18);

transform.reset();
