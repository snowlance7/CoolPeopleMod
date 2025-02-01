using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CoolPeopleMod.Items.RatPlush
{
    // Says "happy birthday rat" from a random person when squeezed
    // For rat it will spawn a rat that guides him to the exit
    // If slayer is in the lobby, doesnt work
    internal class RatPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ulong RatSteamID;
        public ulong SlayerSteamID;
        public AudioSource ItemAudio;
        public AudioClip[] BirthdaySFX;
        public Animator ItemAnimator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    }
}
