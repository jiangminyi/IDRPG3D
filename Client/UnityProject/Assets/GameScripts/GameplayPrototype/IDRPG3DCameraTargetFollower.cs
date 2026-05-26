using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DCameraTargetFollower : MonoBehaviour
    {
        [SerializeField] private float heightOffset;

        private readonly List<IDRPG3DCombatUnit> units = new List<IDRPG3DCombatUnit>(16);

        public int TrackedUnitCountForTest => units.Count;

        public void Configure(IReadOnlyList<IDRPG3DCombatUnit> trackedUnits)
        {
            units.Clear();
            if (trackedUnits == null)
            {
                return;
            }

            for (var i = 0; i < trackedUnits.Count; i++)
            {
                if (ShouldTrack(trackedUnits[i]))
                {
                    units.Add(trackedUnits[i]);
                }
            }
        }

        public void AddUnits(IReadOnlyList<IDRPG3DCombatUnit> trackedUnits)
        {
            if (trackedUnits == null)
            {
                return;
            }

            for (var i = 0; i < trackedUnits.Count; i++)
            {
                AddUnit(trackedUnits[i]);
            }
        }

        public void AddUnit(IDRPG3DCombatUnit unit)
        {
            if (!ShouldTrack(unit) || units.Contains(unit))
            {
                return;
            }

            units.Add(unit);
        }

        private void LateUpdate()
        {
            TickForTest();
        }

        public void TickForTest()
        {
            if (!TryCalculateCenter(out var center))
            {
                return;
            }

            transform.position = center + Vector3.up * heightOffset;
        }

        private bool TryCalculateCenter(out Vector3 center)
        {
            center = Vector3.zero;
            var count = 0;
            for (var i = units.Count - 1; i >= 0; i--)
            {
                var unit = units[i];
                if (unit == null)
                {
                    units.RemoveAt(i);
                    continue;
                }

                if (!ShouldTrack(unit) || !unit.IsAlive)
                {
                    units.RemoveAt(i);
                    continue;
                }

                center += unit.transform.position;
                count++;
            }

            if (count <= 0)
            {
                return false;
            }

            center /= count;
            return true;
        }

        private static bool ShouldTrack(IDRPG3DCombatUnit unit)
        {
            return unit != null && unit.Faction == IDRPG3DCombatFaction.Hero;
        }
    }
}
