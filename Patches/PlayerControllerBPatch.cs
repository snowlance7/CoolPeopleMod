using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using UnityEngine;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource logger = LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.Update))]
        private static void UpdatePostfix(ref bool ___inTerminalMenu, ref Transform ___thisPlayerBody, ref float ___fallValue)
        {
            if (StatusEffectController.Instance.infiniteSprintSeconds > 0) { localPlayer.sprintMeter = StatusEffectController.Instance.freezeSprintMeter; }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControllerB.Update))]
        private static void UpdatePrefix()
        {
            if (StatusEffectController.Instance.statusNegationSeconds > 0)
            {
                localPlayer.bleedingHeavily = false;
                localPlayer.criticallyInjured = false;
                localPlayer.isMovementHindered = 0;
                localPlayer.isExhausted = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
        private static void DamagePlayerPrefix(ref int damageNumber, ref CauseOfDeath causeOfDeath)
        {
            try
            {
                if (StatusEffectController.Instance.damageReductionSeconds > 0)
                {
                    logger.LogDebug("Applying " + StatusEffectController.Instance.damageReductionPercent + "% damage reduction");
                    float reductionPercent = StatusEffectController.Instance.damageReductionPercent / 100.0f;
                    int reductionAmount = Convert.ToInt32(damageNumber * reductionPercent);
                    int damageAfterReduction = damageNumber - reductionAmount;
                    logger.LogDebug($"Initial damage: {damageNumber}, Damage reduction: {reductionAmount}, damage after reduction: {damageAfterReduction}");
                    damageNumber = damageAfterReduction;
                }
                if (StatusEffectController.Instance.bulletProofMultiplier != 0 && causeOfDeath == CauseOfDeath.Gunshots)
                {
                    float reductionPercent = StatusEffectController.Instance.bulletProofMultiplier * .10f;
                    int reductionAmount = (int)(damageNumber * reductionPercent);
                    int damageAfterReduction = damageNumber - reductionAmount;
                    logger.LogDebug($"Initial damage: {damageNumber}, Damage reduction: {reductionAmount}, damage after reduction: {damageAfterReduction}");
                    damageNumber = damageAfterReduction;
                }
            }
            catch (Exception e)
            {
                logger.LogError("Error in DamagePlayerPrefix: " + e);
                return;
            }
        }
    }
}