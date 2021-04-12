==================================================
====================READ.ME=======================
==================================================
Created by: Ben Veldhuizen (5981778) 
        and Roos Boekelman (5681936)


This is a buoyancy simulator. 
The simulation is currently running with seawater, but the density/fluid can be changed by choosing a different fluid in ObjectBehaviour.cs (line13-17) and adding this to the buoyancy function on line 42.


When running the program you can spawn objects by left-clicking for a cube and right-clicking for a cylinder.
You can change the mass of the next spawning object my pressing the following keys:
'u'     : Increase mass by 1000 per press
'i'     : decrease mass by 1000 per press
'j'     : Increase mass by 100 every frame (hold) - faster
'k'     : decrease mass by 100 every frame (hold) - faster
'Space' : Deletes all spawned objects.
'esc'   : closes the application if in build mode.

The camera can be controlled with the following options
WASD/Arrows:  Movement
Q:            Climb
E:            Drop
Shift:        Move faster
Control:      Move slower
End:          Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).

Disclaimer: A water texture has been added. However the waves shown by this texture are not incorporated in the physics. Instead we can see water as a horizontal plane on y = 0.