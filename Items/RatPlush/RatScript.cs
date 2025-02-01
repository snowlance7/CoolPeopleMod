using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace CoolPeopleMod.Items.RatPlush
{
    internal class RatScript : NetworkBehaviour
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioSource RatAudio;
        public AudioClip BirthdayMixtapeSFX;
        public NavMeshAgent agent;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    }
}
