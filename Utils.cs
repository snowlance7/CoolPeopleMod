using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod
{
    internal static class Utils
    {
        private static ManualLogSource logger = LoggerInstance;

        public static bool CalculatePath(Vector3 fromPos, Vector3 toPos)
        {
            Vector3 from = RoundManager.Instance.GetNavMeshPosition(fromPos, RoundManager.Instance.navHit, 1.75f);
            Vector3 to = RoundManager.Instance.GetNavMeshPosition(toPos, RoundManager.Instance.navHit, 1.75f);

            NavMeshPath path = new();
            return NavMesh.CalculatePath(from, to, -1, path) && Vector3.Distance(path.corners[path.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(to, RoundManager.Instance.navHit, 2.7f)) <= 1.55f; // TODO: Test this
        }

        public static SpawnableEnemyWithRarity getEnemyByName(string name)
        {
            List<SpawnableEnemyWithRarity> allenemies = new List<SpawnableEnemyWithRarity>();

            foreach (var level in StartOfRound.Instance.levels)
            {
                allenemies = allenemies
                    .Union(level.Enemies)
                    .Union(level.OutsideEnemies)
                    .Union(level.DaytimeEnemies)
                    .ToList();
            }
            allenemies = allenemies
            .GroupBy(x => x.enemyType.enemyName)
            .Select(g => g.First())
            .OrderBy(x => x.enemyType.enemyName)
            .ToList();
            SpawnableEnemyWithRarity enemy = allenemies.FirstOrDefault(x => x.enemyType.enemyName == name);
            if (enemy == null)
            { //do original method as backup
                foreach (SelectableLevel level in StartOfRound.Instance.levels)
                {

                    enemy = level.Enemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                    if (enemy == null)
                        enemy = level.DaytimeEnemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                    if (enemy == null)
                        enemy = level.OutsideEnemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());


                }
            }
            if (enemy == null)
            {
                logger.LogWarning($"Enemy '{name}' not found. Available enemies: {string.Join(", ", allenemies.Select(e => e.enemyType.enemyName))}"); return null;
            }
            return enemy;
        }
    }
}
