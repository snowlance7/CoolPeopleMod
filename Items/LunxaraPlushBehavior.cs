using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// For Others, generates 1 group credit per use but has a chance to disconnect player from the game
// For Lunxara, she can store a point, and use it again to teleport back but for 30 seconds it rubberbands her


namespace CoolPeopleMod.Items
{
    internal class LunxaraPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    }
}
