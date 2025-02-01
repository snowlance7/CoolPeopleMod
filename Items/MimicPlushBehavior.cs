using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CoolPeopleMod.Items
{
    // Does a random plushie effect
    // default spawns a mimic on the player
    internal class MimicPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public AudioClip SacrificerSFX;
        public AudioClip[] HugSFX;
        public AudioClip BaldSFX;
        public AudioClip[] BirthdaySFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    }
}
