using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items.PinataPlush
{
    internal class PinataPlushBehavior : PhysicsProp
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public AudioClip AttackSFX;
        public AudioClip PartySFX;
        public AudioClip ShutUpSnowySFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        int damage = 5;
        public static float explodeChance = 0.2f;
        public static float shutUpSnowyChance = 0.01f;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                if (playerHeldBy.playerSteamId == SnowySteamID || TESTING.testing)
                {
                    ItemAnimator.SetTrigger("squeeze");
                    ItemAudio.PlayOneShot(PartySFX);
                    ActivateRandomCandyEffect();
                }
                else
                {
                    ItemAnimator.SetTrigger("headbutt");
                }
            }
        }

        public void DamagePlayerHeldBy()
        {
            ItemAudio.PlayOneShot(AttackSFX);
            playerHeldBy.DamagePlayer(damage, false, false, CauseOfDeath.Bludgeoning, 7, false, playerHeldBy.transform.forward * 10f);
        }

        void ActivateRandomCandyEffect()
        {
            if (playerHeldBy != localPlayer) { return; }

            if (UnityEngine.Random.Range(0f, 1f) <= shutUpSnowyChance)
            {
                ShutUpSnowyServerRpc();
                return;
            }

            int index = UnityEngine.Random.Range(0, 7);

            switch (index)
            {
                case 0:
                    HUDManager.Instance.DisplayTip("Blue", "");
                    StatusEffectController.Instance.HealPlayer(30, true);

                    break;
                case 1:
                    HUDManager.Instance.DisplayTip("Green", "");
                    StatusEffectController.Instance.StatusNegation(30);
                    StatusEffectController.Instance.HealthRegen(1, 80);

                    break;
                case 2:
                    HUDManager.Instance.DisplayTip("Purple", "");
                    StatusEffectController.Instance.DamageReduction(15, 20, true);
                    StatusEffectController.Instance.HealthRegen(2, 10);

                    break;
                case 3:
                    HUDManager.Instance.DisplayTip("Red", "");
                    StatusEffectController.Instance.HealthRegen(9, 5);

                    break;
                case 4:
                    HUDManager.Instance.DisplayTip("Yellow", "");
                    StatusEffectController.Instance.RestoreStamina(25);
                    StatusEffectController.Instance.InfiniteSprint(8);
                    StatusEffectController.Instance.IncreasedMovementSpeed(8, 2, true, true);

                    break;
                case 5:
                    HUDManager.Instance.DisplayTip("Pink", "");
                    
                    if (UnityEngine.Random.Range(0f, 1f) > explodeChance) { break; }
                    Landmine.SpawnExplosion(transform.position, spawnExplosionEffect: true, 5.7f, 6f);

                    break;
                case 6:
                    HUDManager.Instance.DisplayTip("Rainbow", "");
                    StatusEffectController.Instance.HealPlayer(15);
                    StatusEffectController.Instance.InfiniteSprint(5, true);
                    StatusEffectController.Instance.bulletProofMultiplier += 1;
                    StatusEffectController.Instance.StatusNegation(10);
                    StatusEffectController.Instance.HealPlayer(20, true);

                    break;
                default:
                    break;
            }
        }

        IEnumerator ShutUpSnowyCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Landmine.SpawnExplosion(transform.position, spawnExplosionEffect: true, 5.7f, 6f);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShutUpSnowyServerRpc()
        {
            if (!IsServerOrHost) { return; }
            ShutUpSnowyClientRpc();
        }

        [ClientRpc]
        public void ShutUpSnowyClientRpc()
        {
            ItemAudio.PlayOneShot(ShutUpSnowySFX);
            StartCoroutine(ShutUpSnowyCoroutine(3f));
            
        }
    }
}
