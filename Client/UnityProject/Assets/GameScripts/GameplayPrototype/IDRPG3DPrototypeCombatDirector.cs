using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public static class IDRPG3DPrototypeCombatDirector
    {
        public static bool TryFindLowestHealthAlly(
            IDRPG3DCombatUnit source,
            float radius,
            bool includeSelf,
            out IDRPG3DCombatUnit ally)
        {
            ally = null;
            var bestRatio = 1f;
            var units = FindUnits();
            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (candidate == null || !candidate.IsAlive || candidate.Faction != source.Faction)
                {
                    continue;
                }

                if (!includeSelf && candidate == source)
                {
                    continue;
                }

                if (radius > 0f && (candidate.transform.position - source.transform.position).sqrMagnitude > radius * radius)
                {
                    continue;
                }

                var ratio = candidate.MaxHealth > 0f ? candidate.Health / candidate.MaxHealth : 1f;
                if (ratio >= bestRatio || ratio >= 0.7f)
                {
                    continue;
                }

                bestRatio = ratio;
                ally = candidate;
            }

            return ally != null;
        }

        public static bool TryFindDeadAlly(IDRPG3DCombatUnit source, float radius, out IDRPG3DCombatUnit ally)
        {
            ally = null;
            var units = FindUnits();
            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (candidate == null || candidate.IsAlive || candidate.Faction != source.Faction || candidate == source)
                {
                    continue;
                }

                if (radius > 0f && (candidate.transform.position - source.transform.position).sqrMagnitude > radius * radius)
                {
                    continue;
                }

                ally = candidate;
                return true;
            }

            return false;
        }

        public static void FindAreaEnemies(IDRPG3DCombatUnit source, Vector3 center, float radius, List<IDRPG3DCombatUnit> results)
        {
            results.Clear();
            var units = FindUnits();
            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (candidate == null || !candidate.IsAlive || candidate.Faction == source.Faction)
                {
                    continue;
                }

                if ((candidate.transform.position - center).sqrMagnitude <= radius * radius)
                {
                    results.Add(candidate);
                }
            }
        }

        public static bool TryFindNearestEnemy(IDRPG3DCombatUnit source, float radius, out IDRPG3DCombatUnit enemy)
        {
            enemy = null;
            if (source == null)
            {
                return false;
            }

            var bestSqrDistance = float.MaxValue;
            var maxSqrDistance = radius > 0f ? radius * radius : float.MaxValue;
            var units = FindUnits();
            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (candidate == null || !candidate.IsAlive || candidate.Faction == source.Faction)
                {
                    continue;
                }

                var sqrDistance = (candidate.transform.position - source.transform.position).sqrMagnitude;
                if (sqrDistance > maxSqrDistance || sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                bestSqrDistance = sqrDistance;
                enemy = candidate;
            }

            return enemy != null;
        }

        public static int CalculatePartyReferenceLevel(IReadOnlyList<IDRPG3DCombatUnit> heroes)
        {
            if (heroes == null || heroes.Count == 0)
            {
                return 1;
            }

            var sum = 0;
            var count = 0;
            for (var i = 0; i < heroes.Count; i++)
            {
                if (heroes[i] == null)
                {
                    continue;
                }

                sum += Mathf.Max(1, heroes[i].Level);
                count++;
            }

            return count > 0 ? Mathf.Max(1, Mathf.RoundToInt((float)sum / count)) : 1;
        }

        private static IDRPG3DCombatUnit[] FindUnits()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<IDRPG3DCombatUnit>(FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<IDRPG3DCombatUnit>();
#endif
        }
    }
}
