using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items.RatPlush
{
    // Says "happy birthday rat" from a random person when squeezed
    // For rat it will spawn a rat that guides him to the exit
    // If slayer is in the lobby, doesnt work
    internal class RatPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioSource ItemAudio;
        public AudioClip[] BirthdaySFX;
        public Animator ItemAnimator;
        public GameObject RatPrefab;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        RatScript? currentRat;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                RoundManager.PlayRandomClip(ItemAudio, BirthdaySFX);
                ItemAnimator.SetTrigger("squeeze");

                if (IsSlayerInGame()) { return; }

                if (playerHeldBy.playerSteamId == RatSteamID || TESTING.testing)
                {
                    TrySpawnRatOnServer();
                }
            }
        }

        void TrySpawnRatOnServer()
        {
            if (!IsServerOrHost) { return; }
            if (!playerHeldBy.isInsideFactory) { return; }
            Vector3 mainEntrancePosition = RoundManager.FindMainEntrancePosition(true);
            if (!Utils.CalculatePath(playerHeldBy.transform.position, mainEntrancePosition)) { return; }
            if (currentRat != null)
            {
                currentRat.DisappearClientRpc();
            }

            float forwardOffset = 1f;
            Vector3 positionInFrontOfPlayer = playerHeldBy.transform.position + playerHeldBy.transform.forward * forwardOffset;

            currentRat = GameObject.Instantiate(RatPrefab, positionInFrontOfPlayer, playerHeldBy.transform.rotation).GetComponent<RatScript>();
            currentRat.NetworkObject.Spawn(true);
        }

        bool IsSlayerInGame()
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerSteamId == SlayerSteamID)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
