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
        public ulong GlitchSteamID;
        public ulong RodrigoSteamID;
        public Animator ItemAnimator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        Ray grenadeThrowRay;
        RaycastHit grenadeHit;
        int stunGrenadeMask = 268437761;

        bool isThrown;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (playerHeldBy.playerSteamId == snowySteamID || playerHeldBy.playerSteamId == GlitchSteamID || TESTING.testing)
            {
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
            // Log initial state
            logger.LogDebug($"cFallWithCurve called. Start Position: {startFallingPosition}, Target Position: {targetFloorPosition}, Initial cfallTime: {fallTime}");

            float magnitude = (startFallingPosition - targetFloorPosition).magnitude;
            logger.LogDebug($"Calculated magnitude: {magnitude}");

            // Log rotation interpolation
            Quaternion targetRotation = Quaternion.Euler(itemProperties.restingRotation.x, base.transform.eulerAngles.y, itemProperties.restingRotation.z);
            base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, 14f * Time.deltaTime / magnitude);
            logger.LogDebug($"Updated rotation to: {base.transform.rotation.eulerAngles}");

            // Log position interpolation for primary fall
            base.transform.localPosition = Vector3.Lerp(startFallingPosition, targetFloorPosition, grenadeFallCurve.Evaluate(fallTime));
            logger.LogDebug($"Updated primary fall position to: {base.transform.localPosition}");

            // Conditional logging for vertical fall curve
            if (magnitude > 5f)
            {
                logger.LogDebug("Magnitude > 5, using grenadeVerticalFallCurveNoBounce.");
                base.transform.localPosition = Vector3.Lerp(
                    new Vector3(base.transform.localPosition.x, startFallingPosition.y, base.transform.localPosition.z),
                    new Vector3(base.transform.localPosition.x, targetFloorPosition.y, base.transform.localPosition.z),
                    grenadeVerticalFallCurveNoBounce.Evaluate(fallTime)
                );
            }
            else
            {
                logger.LogDebug("Magnitude <= 5, using grenadeVerticalFallCurve.");
                base.transform.localPosition = Vector3.Lerp(
                    new Vector3(base.transform.localPosition.x, startFallingPosition.y, base.transform.localPosition.z),
                    new Vector3(base.transform.localPosition.x, targetFloorPosition.y, base.transform.localPosition.z),
                    grenadeVerticalFallCurve.Evaluate(fallTime)
                );
            }

            // Log updated position and fallTime
            logger.LogDebug($"Updated local position after vertical fall: {base.transform.localPosition}");

            fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
            logger.LogDebug($"Updated cfallTime to: {fallTime}");
        }

        public override void OnHitGround()
        {
            base.OnHitGround();
            logger.LogDebug("Hit ground");

            if (!isThrown) { return; }
            Landmine.SpawnExplosion(transform.position, true);
            isThrown = false;
        }
    }
}
