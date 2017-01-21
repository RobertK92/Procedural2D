using FarseerPhysics.Dynamics;
using System;

namespace Game1
{
    [Flags]
    public enum PhysicsLayers
    {
        All                     = Category.All,
        Player                  = Category.Cat1,
        WorldObjects            = Category.Cat2,
        WorldObjectSensors      = Category.Cat3
    }
}
