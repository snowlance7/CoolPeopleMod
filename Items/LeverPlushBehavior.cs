﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// For Nut, returns everyone to ship without their items and pulls the lever
// For everyone else, randomizes their suit

namespace CoolPeopleMod.Items
{
    internal class LeverPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    }
}