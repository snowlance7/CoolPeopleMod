using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// if used by rodrigo, throw like a grenade and explode
// if used by anyone else, spawn a seamine field around them

namespace CoolPeopleMod.Items
{
    internal class RedrigoPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public GameObject SeaminePrefab;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        
    }
}