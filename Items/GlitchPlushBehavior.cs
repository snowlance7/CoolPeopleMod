using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod
{
    internal class GlitchPlushBehavior : PhysicsProp
    {
        private static ManualLogSource logger = LoggerInstance;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AnimationCurve grenadeFallCurve;
        public AnimationCurve grenadeVerticalFallCurve;
        public AnimationCurve grenadeVerticalFallCurveNoBounce;
        public AudioSource ItemAudio;
        public AudioClip BaldSFX;
        public AudioClip DefaultSFX;
        public AudioClip RodrigoSFX;
        public Animator ItemAnimator;
        public Material HairMat;
        public SkinnedMeshRenderer HairRenderer;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        System.Random? random;
        bool initializedRandomSeed;

        Ray grenadeThrowRay;
        RaycastHit grenadeHit;
        int stunGrenadeMask = 268437761;
        int explodeGlitchChance = 500;

        bool isThrown;

        public override void Start()
        {
            base.Start();

            if (!RoundManager.Instance.hasInitializedLevelRandomSeed)
            {
                RoundManager.Instance.InitializeRandomNumberGenerators();
            }
            logger.LogDebug("initialized random number generators");

            if (localPlayer.playerSteamId == GlitchSteamID || TESTING.testing)
            {
                HairRenderer.material = HairMat;
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (playerHeldBy.playerSteamId == SnowySteamID || playerHeldBy.playerSteamId == GlitchSteamID || TESTING.testing)
            {
                if (!initializedRandomSeed)
                {
                    int seed = StartOfRound.Instance.randomMapSeed + 158;
                    logger.LogDebug("Assigning new random with seed: " + seed);
                    random = new System.Random(seed);
                    initializedRandomSeed = true;
                }

                if (random!.Next(1, explodeGlitchChance + 1) == 1)
                {
                    Landmine.SpawnExplosion(transform.position, spawnExplosionEffect: true, 5.7f, 6f);
                    return;
                }

                isThrown = true;
                playerHeldBy.DiscardHeldObject(placeObject: true, null, GetGrenadeThrowDestination(playerHeldBy.gameplayCamera.transform));
                ItemAudio.PlayOneShot(BaldSFX);
            }
            else if (playerHeldBy.playerSteamId == RodrigoSteamID)
            {
                ItemAnimator.SetTrigger("squeeze");
                ItemAudio.PlayOneShot(RodrigoSFX);
            }
            else
            {
                ItemAnimator.SetTrigger("squeeze");
                ItemAudio.PlayOneShot(DefaultSFX);
            }
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

        public override void FallWithCurve()
        {
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

            // Log updated position and fallTime

            fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
        }

        public override void OnHitGround()
        {
            base.OnHitGround();

            if (!isThrown) { return; }
            Landmine.SpawnExplosion(transform.position + Vector3.up, spawnExplosionEffect: true, 1f, 5.7f);
            isThrown = false;
        }
    }
}
