using System.Collections;
using UnityEngine;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items
{
    internal class SCP999PlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public AudioClip[] HugSFX;
        public AudioClip[] RoamSFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        Coroutine? hugRoutine;

        public static int healthPerSecond = 2;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                if (playerHeldBy.playerSteamId == LizzieSteamID || playerHeldBy.playerSteamId == SnowySteamID || TESTING.testing)
                {
                    RoundManager.PlayRandomClip(ItemAudio, HugSFX);
                    if (hugRoutine == null)
                    {
                        hugRoutine = StartCoroutine(HugCoroutine());
                    }
                }
                else
                {
                    ItemAnimator.SetTrigger("squeeze");
                    RoundManager.PlayRandomClip(ItemAudio, RoamSFX);
                }
            }
        }

        IEnumerator HugCoroutine()
        {
            ItemAnimator.SetBool("hug", true);

            while (playerHeldBy != null && playerHeldBy.health < 100)
            {
                playerHeldBy.health += healthPerSecond;
                
                if (playerHeldBy == localPlayer)
                {
                    HUDManager.Instance.UpdateHealthUI(localPlayer.health, false);
                }

                yield return new WaitForSecondsRealtime(1f);
            }

            yield return new WaitForSecondsRealtime(3f);

            ItemAnimator.SetBool("hug", false);
            hugRoutine = null;
        }
    }
}
