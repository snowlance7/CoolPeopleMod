using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items
{
    internal class FunoPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public AudioClip[] FunoSFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                ItemAnimator.SetTrigger("squeeze");
                RoundManager.PlayRandomClip(ItemAudio, FunoSFX);

                if (playerHeldBy.playerSteamId == GlitchSteamID || playerHeldBy.playerSteamId == FunoSteamID || TESTING.testing)
                BlowUpGlitch();
            }
        }

        void BlowUpGlitch()
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player != null && player.playerSteamId == GlitchSteamID)
                {
                    Landmine.SpawnExplosion(player.transform.position + Vector3.up, spawnExplosionEffect: true, 5.7f, 6f);
                }
            }
        }
    }
}
