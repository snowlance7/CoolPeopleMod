using BepInEx.Logging;
using CoolPeopleMod.Items.RatPlush;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items.DiceMimic
{
    // Does a random plushie effect
    // default spawns a mimic on the player
    internal class DiceMimicPlushBehavior : PhysicsProp
    {
        private static ManualLogSource logger = LoggerInstance;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Animator ItemAnimator;
        public AudioSource ItemAudio;
        public AudioClip DiceSFX;
        public Spinner DiceScript;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        System.Random? random;
        bool initializedRandomSeed;

        bool isThrown;

        const int plushieCount = 5;
        int currentBehavior;

        public override void Start()
        {
            base.Start();

            if (!RoundManager.Instance.hasInitializedLevelRandomSeed)
            {
                RoundManager.Instance.InitializeRandomNumberGenerators();
            }
            logger.LogDebug("initialized random number generators");
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                ItemAudio.PlayOneShot(DiceSFX);
                ItemAnimator.SetTrigger("squeeze");
                DiceScript.StartHyperSpinning(3f);

                if (IsRatInGame()) { return; }

                if (playerHeldBy.playerSteamId == SlayerSteamID || playerHeldBy.playerSteamId == SnowySteamID || TESTING.testing)
                {
                    DoRandomPlushieEffect();
                }
                else
                {
                    SpawnMimic();
                }
            }
        }

        void SpawnMimic()
        {
            if (!IsServerOrHost || !TESTING.mimic) { return; }
            GameObject gameObject = Instantiate(Utils.getEnemyByName("Masked").enemyType.enemyPrefab, playerHeldBy.transform.position, Quaternion.identity);
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
        }

        bool IsRatInGame()
        {
            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerSteamId == RatSteamID)
                {
                    return true;
                }
            }

            return false;
        }

        void DoRandomPlushieEffect()
        {
            if (!initializedRandomSeed)
            {
                int seed = StartOfRound.Instance.randomMapSeed + 158;
                logger.LogDebug("Assigning new random with seed: " + seed);
                random = new System.Random(seed);
                initializedRandomSeed = true;
            }

            currentBehavior = random!.Next(1, plushieCount + 1);

            switch (currentBehavior)
            {
                case 1:
                    DoGlitchPlushBehavior();
                    break;
                case 2:
                    DoPinataPlushBehavior();
                    break;
                case 3:
                    DoSCP999PlushBehavior();
                    break;
                case 4:
                    DoRatPlushBehavior();
                    break;
                case 5:
                    DoFunoPlushBehavior();
                    break;
                default:
                    break;
            }
        }

        public override void OnHitGround()
        {
            switch (currentBehavior)
            {
                case 1: // GlitchPlushBehavior

                    if (isThrown)
                    {
                        Landmine.SpawnExplosion(transform.position + Vector3.up, spawnExplosionEffect: true, 5.7f, 6f);
                        isThrown = false;
                    }
                    break;
                default:
                    base.OnHitGround();
                    break;
            }
        }

        public override void FallWithCurve()
        {
            switch (currentBehavior)
            {
                case 1: // GlitchPlushBehavior

                    float magnitude = (startFallingPosition - targetFloorPosition).magnitude;

                    // Log rotation interpolation
                    Quaternion targetRotation = Quaternion.Euler(itemProperties.restingRotation.x, base.transform.eulerAngles.y, itemProperties.restingRotation.z);
                    base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, 14f * Time.deltaTime / magnitude);

                    // Log position interpolation for primary fall
                    base.transform.localPosition = Vector3.Lerp(startFallingPosition, targetFloorPosition, grenadeFallCurve.Evaluate(fallTime));

                    // Conditional logging for vertical fall curve
                    if (magnitude > 5f)
                    {
                        base.transform.localPosition = Vector3.Lerp(
                            new Vector3(base.transform.localPosition.x, startFallingPosition.y, base.transform.localPosition.z),
                            new Vector3(base.transform.localPosition.x, targetFloorPosition.y, base.transform.localPosition.z),
                            grenadeVerticalFallCurveNoBounce.Evaluate(fallTime)
                        );
                    }
                    else
                    {
                        base.transform.localPosition = Vector3.Lerp(
                            new Vector3(base.transform.localPosition.x, startFallingPosition.y, base.transform.localPosition.z),
                            new Vector3(base.transform.localPosition.x, targetFloorPosition.y, base.transform.localPosition.z),
                            grenadeVerticalFallCurve.Evaluate(fallTime)
                        );
                    }

                    fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
                    break;
                default:
                    base.FallWithCurve();
                    break;
            }
        }

        #region GlitchPlushBehavior
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AnimationCurve grenadeFallCurve;
        public AnimationCurve grenadeVerticalFallCurve;
        public AnimationCurve grenadeVerticalFallCurveNoBounce;
        public AudioClip BaldSFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        Ray grenadeThrowRay;
        RaycastHit grenadeHit;
        int stunGrenadeMask = 268437761;

        void DoGlitchPlushBehavior()
        {
            isThrown = true;
            playerHeldBy.DiscardHeldObject(placeObject: true, null, GetGrenadeThrowDestination(playerHeldBy.gameplayCamera.transform));
            ItemAudio.PlayOneShot(BaldSFX);
        }

        public Vector3 GetGrenadeThrowDestination(Transform ejectPoint)
        {
            Vector3 position = base.transform.position;
            Debug.DrawRay(ejectPoint.position, ejectPoint.forward, Color.yellow, 15f);
            grenadeThrowRay = new Ray(ejectPoint.position, ejectPoint.forward);
            position = ((!Physics.Raycast(grenadeThrowRay, out grenadeHit, 12f, stunGrenadeMask, QueryTriggerInteraction.Ignore)) ? grenadeThrowRay.GetPoint(10f) : grenadeThrowRay.GetPoint(grenadeHit.distance - 0.05f));
            Debug.DrawRay(position, Vector3.down, Color.blue, 15f);
            grenadeThrowRay = new Ray(position, Vector3.down);
            if (Physics.Raycast(grenadeThrowRay, out grenadeHit, 30f, stunGrenadeMask, QueryTriggerInteraction.Ignore))
            {
                position = grenadeHit.point + Vector3.up * 0.05f;
            }
            else
            {
                position = grenadeThrowRay.GetPoint(30f);
            }

            position += new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            return position;
        }
        #endregion

        #region PinataPlushBehavior

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioClip PartySFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        void DoPinataPlushBehavior()
        {
            ItemAudio.PlayOneShot(PartySFX);
            ActivateRandomCandyEffect();
        }

        void ActivateRandomCandyEffect()
        {
            if (playerHeldBy != localPlayer) { return; }

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

                    if (UnityEngine.Random.Range(0f, 1f) > PinataPlush.PinataPlushBehavior.explodeChance) { break; }
                    Landmine.SpawnExplosion(localPlayer.transform.position + Vector3.up, spawnExplosionEffect: true, 5.7f, 6f);

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

        #endregion

        #region SCP999PlushBehavior

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioClip[] HugSFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        Coroutine? hugRoutine;

        void DoSCP999PlushBehavior()
        {
            RoundManager.PlayRandomClip(ItemAudio, HugSFX);
            if (hugRoutine == null)
            {
                hugRoutine = StartCoroutine(HugCoroutine());
            }
        }

        IEnumerator HugCoroutine()
        {
            ItemAnimator.SetBool("hug", true);

            while (playerHeldBy != null && playerHeldBy.health < 100)
            {
                playerHeldBy.health += SCP999PlushBehavior.healthPerSecond;

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

        #endregion

        #region RatPlushBehavior

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioClip[] BirthdaySFX;
        public GameObject RatPrefab;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        RatScript? currentRat;

        void DoRatPlushBehavior()
        {
            RoundManager.PlayRandomClip(ItemAudio, BirthdaySFX);
            TrySpawnRatOnServer();
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

        #endregion

        #region FunoPlushBehavior

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioClip[] FunoSFX;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        void DoFunoPlushBehavior()
        {
            RoundManager.PlayRandomClip(ItemAudio, FunoSFX);
            BlowUpGlitch();
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

        #endregion
    }
}
