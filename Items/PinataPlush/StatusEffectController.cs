﻿using BepInEx.Logging;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod
{
    public class StatusEffectController : MonoBehaviour
    {
        private static StatusEffectController _instance;

        public static StatusEffectController Instance
        {
            get
            {
                // If the instance doesn't exist, try to find it in the scene
                if (_instance == null)
                {
                    _instance = FindObjectOfType<StatusEffectController>();

                    // If it's still null, create a new GameObject and add the component
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("SCP-956 StatusEffectController");
                        _instance = singletonObject.AddComponent<StatusEffectController>();
                    }
                }
                return _instance;
            }
        }

        private static ManualLogSource logger = LoggerInstance;

        private Coroutine statusNegationCoroutine;
        private Coroutine damageReductionCoroutine;
        private Coroutine infiniteSprintCoroutine;
        private Coroutine increasedMovementSpeedCoroutine;

        public int statusNegationSeconds = 0;
        public int damageReductionSeconds = 0;
        public int infiniteSprintSeconds = 0;
        public int increasedMovementSpeedSeconds = 0;

        public int damageReductionPercent = 0;
        public int increasedMovementSpeedPercent = 0;

        public float freezeSprintMeter;
        public float baseMovementSpeed = 4.6f;
        public int bulletProofMultiplier = 0;

        private readonly string[] effectNames = { "HealPlayer", "RestoreStamina", "HealthRegen", "StatusNegation", "DamageReduction", "InfiniteSprint", "IncreasedMovementSpeed" };
        private Dictionary<string, MethodInfo> effectMethods = null!;
        
        public float pitchAdjust;


        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // If an instance already exists and it's not this one, destroy this instance
                if (this != _instance)
                {
                    Destroy(gameObject);
                }
            }

            effectMethods = new Dictionary<string, MethodInfo>();
            foreach (var method in typeof(StatusEffectController).GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (effectNames.Contains(method.Name))
                {
                    effectMethods[method.Name] = method;
                }
            }
            logger.LogDebug($"Effect methods: {string.Join(", ", effectMethods.Keys)}");
        }

        public void ApplyCandyEffects(string config)
        {
            // Split the config string into individual effect strings
            var effectStrings = config.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var effectString in effectStrings)
            {
                // Split the effect string into the effect name and its parameters
                var parts = effectString.Split(new[] { ':' }, 2);
                if (parts.Length != 2) continue; // Skip if the effect string is invalid

                var effectName = parts[0].Trim();
                var parameters = parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // Check if the effect method exists
                if (!effectMethods.TryGetValue(effectName, out var methodInfo)) continue;

                try
                {
                    // Parse parameters and match with method signature
                    var methodParams = methodInfo.GetParameters();
                    var parsedParams = new object[methodParams.Length];

                    for (int i = 0; i < methodParams.Length; i++)
                    {
                        if (i < parameters.Length)
                        {
                            parsedParams[i] = Convert.ChangeType(parameters[i], methodParams[i].ParameterType);
                        }
                        else if (methodParams[i].IsOptional)
                        {
                            parsedParams[i] = methodParams[i].DefaultValue;
                        }
                        else
                        {
                            throw new ArgumentException("Not enough parameters provided for non-optional parameters.");
                        }
                    }

                    // Invoke the method with the parsed parameters
                    methodInfo.Invoke(Instance, parsedParams);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    logger.LogError($"Failed to apply effect {effectName}: {ex.Message}");
                    continue;
                }
            }
        }

        // Status effects

        public void HealPlayer(int amount, bool overHeal = false)
        {
            logger.LogDebug("Healing: " + amount);
            PlayerControllerB player = localPlayer;

            player.MakeCriticallyInjured(false);

            int newHealth = player.health + amount;

            if (newHealth > 100 && !overHeal) { newHealth = 100; }
            player.health = newHealth;
            HUDManager.Instance.UpdateHealthUI(newHealth, false);
        }

        public void RestoreStamina(int percent)
        {
            float percentage = percent / 100.0f;
            float newStamina = localPlayer.sprintMeter + (1 * percentage);
            localPlayer.sprintMeter = newStamina;
        }

        public void HealthRegen(int hpPerSecond, int seconds)
        {
            StartCoroutine(HealthRegenCoroutine(hpPerSecond, seconds));
        }

        public void StatusNegation(int seconds, bool timeStackable = false)
        {
            if (statusNegationCoroutine != null)
            {
                if (timeStackable) { statusNegationSeconds += seconds; return; }
                StopCoroutine(statusNegationCoroutine);
            }
            statusNegationCoroutine = StartCoroutine(StatusNegationCoroutine(seconds));
        }

        public void DamageReduction(int seconds, int percent, bool timeStackable = false, bool stackable = false)
        {
            if (damageReductionCoroutine != null)
            {
                if (timeStackable) { damageReductionSeconds += seconds; }
                if (stackable) { damageReductionPercent += percent; }
                if (timeStackable || stackable) { return; }
                StopCoroutine(damageReductionCoroutine);
            }
            damageReductionCoroutine = StartCoroutine(DamageReductionCoroutine(seconds, percent));
        }

        public void InfiniteSprint(int seconds, bool timeStackable = false)
        {
            if (infiniteSprintCoroutine != null)
            {
                if (timeStackable) { infiniteSprintSeconds += seconds; return; }
                StopCoroutine(infiniteSprintCoroutine);
            }
            infiniteSprintCoroutine = StartCoroutine(InfiniteSprintCoroutine(seconds));
        }

        public void IncreasedMovementSpeed(int seconds, int percent, bool timeStackable = false, bool stackable = false)
        {
            if (increasedMovementSpeedCoroutine != null)
            {
                if (timeStackable) { increasedMovementSpeedSeconds += seconds; }
                if (stackable) { increasedMovementSpeedPercent += percent; }
                if (timeStackable || stackable) { return; }
                StopCoroutine(increasedMovementSpeedCoroutine);
                if (baseMovementSpeed != 0) { localPlayer.movementSpeed = baseMovementSpeed; }
            }
            increasedMovementSpeedCoroutine = StartCoroutine(IncreasedMovementSpeedCoroutine(seconds, percent));
        }

        public void DamagePlayerOverTime(int damage, int perSeconds, bool untilDead = false, int totalSeconds = 10)
        {
            StartCoroutine(DamagePlayerOverTimeCoroutine(damage, perSeconds, untilDead, totalSeconds));
        }

        // Coroutines

        private IEnumerator HealthRegenCoroutine(int hpPerSecond, int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                HealPlayer(hpPerSecond);
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private IEnumerator StatusNegationCoroutine(int seconds)
        {
            statusNegationSeconds = seconds;
            while (statusNegationSeconds > 0)
            {
                logger.LogDebug("Status Negation: " + statusNegationSeconds);
                statusNegationSeconds--;
                yield return new WaitForSecondsRealtime(1f);
            }
            statusNegationSeconds = 0;
            statusNegationCoroutine = null;
        }

        private IEnumerator DamageReductionCoroutine(int seconds, int percent)
        {
            damageReductionSeconds = seconds;
            damageReductionPercent = percent;
            while (damageReductionSeconds > 0)
            {
                logger.LogDebug("Damage Reduction: " + damageReductionPercent + " " + damageReductionSeconds);
                damageReductionSeconds--;
                yield return new WaitForSecondsRealtime(1f);
            }
            damageReductionPercent = 0;
            damageReductionSeconds = 0;
            damageReductionCoroutine = null;
        }

        private IEnumerator InfiniteSprintCoroutine(int seconds)
        {
            freezeSprintMeter = localPlayer.sprintMeter;
            infiniteSprintSeconds = seconds;
            while (infiniteSprintSeconds > 0)
            {
                infiniteSprintSeconds--;
                logger.LogDebug("Infinite sprint: " + infiniteSprintSeconds);
                yield return new WaitForSecondsRealtime(1f);
            }
            infiniteSprintSeconds = 0;
            infiniteSprintCoroutine = null;
        }

        private IEnumerator IncreasedMovementSpeedCoroutine(int seconds, int percent)
        {
            baseMovementSpeed = localPlayer.movementSpeed;
            increasedMovementSpeedSeconds = seconds;
            increasedMovementSpeedPercent = percent;
            while (increasedMovementSpeedSeconds > 0)
            {
                float movementSpeedMultiplier = 1 + (increasedMovementSpeedPercent / 100.0f);
                localPlayer.movementSpeed = baseMovementSpeed * movementSpeedMultiplier;
                increasedMovementSpeedSeconds--;
                logger.LogDebug("Increased movement speed: " + increasedMovementSpeedPercent + "% " + increasedMovementSpeedSeconds);
                yield return new WaitForSecondsRealtime(1f);
            }
            increasedMovementSpeedPercent = 0;
            localPlayer.movementSpeed = baseMovementSpeed;
            increasedMovementSpeedSeconds = 0;
            increasedMovementSpeedCoroutine = null;
        }

        private IEnumerator DamagePlayerOverTimeCoroutine(int damage, int perSeconds, bool untilDead, int totalSeconds)
        {
            if (untilDead)
            {
                while (!localPlayer.isPlayerDead)
                {
                    localPlayer.inSpecialInteractAnimation = true;
                    localPlayer.DamagePlayer(damage, false);
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                    localPlayer.inSpecialInteractAnimation = false;
                    HUDManager.Instance.UpdateHealthUI(localPlayer.health, false);
                    yield return new WaitForSecondsRealtime(perSeconds);
                }
                yield break;
            }

            int seconds = 0;
            while (seconds < totalSeconds)
            {
                localPlayer.DamagePlayer(damage, false);
                seconds += perSeconds;
                yield return new WaitForSecondsRealtime(perSeconds);
            }
        }
    }
}
