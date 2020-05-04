# Rope Test

This is a test I've created to try out some rope physics on Unity.

Ended up doing a more or less generic component that used DistanceJoint2d and FixedJoint2d to achieve the rope effect.

![Image](https://github.com/DiogoDeAndrade/RopeTest/raw/master/Screenshots/screen01.png)

## Tech stuff

The Rope component just needs to be added to the "anchor point" of the rope, and the connectedObject property set to the object that we want to tie to the object.
Then, the component creates a distance joint 2d (with default parameters, some adjustments would have to be done on this if you want to fix the distance, or use a sprint joint, etc) and uses that for the swinging motion using regular Unity physics. Then, a ray is cast into the scene between the anchor point and the object, if it collides with something, the distance joint is replaced by a fixed joint, and a new object is created with a distance joint between the intersection point and the object, and so forth.

This is reverted when there is "line of sight" between the previous joint and the connected object, accounting for co-linearity between the previous segment and the current one.

The code ended up not being too complex, but it's quite cool, even in the current form.

## Credits

* Code [Diogo Andrade]
* Spike ball art by [Julien]
* Chain element by [Fun Punch Games]

## Licenses

All code in this repo is made available through the [GPLv3] license.

Spike ball: © 2005-2013 Julien Jorge <julien.jorge@stuff-o-matic.com>
Chain Element: © 2017 Fun Punch Games Ltd. 

## Metadata

* Autor: [Diogo Andrade][]

[Diogo Andrade]:https://github.com/DiogoDeAndrade
[Fun Punch Games]:https://funpunchgames.com/
[Julien]:https://opengameart.org/content/spike-ball
[GPLv3]:https://www.gnu.org/licenses/gpl-3.0.en.html
[CC-BY-SA 3.0.]:http://creativecommons.org/licenses/by-sa/3.0/
[CC BY-NC-SA 4.0]:https://creativecommons.org/licenses/by-nc-sa/4.0/