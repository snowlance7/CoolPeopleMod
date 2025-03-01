using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Analytics;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items
{
    internal class FunoPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public AudioClip[] FunoSFX;
        public Material BlackMat;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static float pushDistance = 5f;
        public static float pushForce = 50f;

        public override void Start()
        {
            base.Start();

            if (localPlayer.playerSteamId == GlitchSteamID || TESTING.testing)
            {
                mainObjectRenderer.material = BlackMat;
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                ItemAnimator.SetTrigger("squeeze");
                RoundManager.PlayRandomClip(ItemAudio, FunoSFX);

                if (playerHeldBy.playerSteamId == FunoSteamID || playerHeldBy.playerSteamId == SnowySteamID || TESTING.testing)
                {
                    Landmine.SpawnExplosion(playerHeldBy.transform.position + Vector3.up, true, 0f, 5f, 1, 100f);
                    PushNearbyPlayers();
                }
                else if (playerHeldBy.playerSteamId == GlitchSteamID)
                {
                    Landmine.SpawnExplosion(transform.position, true, 1f, 5f, 50, 100f);
                }
            }
        }

        Vector3 GetDirectionToPlayer(PlayerControllerB player)
        {
            return (player.transform.position - transform.position).normalized;
        }

        public void PushNearbyPlayers()
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!player.isPlayerControlled || player == playerHeldBy) { continue; }
                if (Vector3.Distance(transform.position, player.transform.position) > pushDistance) { continue; }

                Vector3 pushDirection = GetDirectionToPlayer(player);
                player.playerRigidbody.isKinematic = false;
                player.playerRigidbody.velocity = Vector3.zero;
                player.externalForceAutoFade += pushDirection * pushForce;
                DropBabyIfHolding(player);

                player.playerRigidbody.isKinematic = true;
            }
        }

        void DropBabyIfHolding(PlayerControllerB player)
        {
            if (player.currentlyHeldObjectServer != null && player.currentlyHeldObjectServer.itemProperties.name == "CaveDwellerBaby")
            {
                player.DiscardHeldObject();
            }
        }
    }
}
