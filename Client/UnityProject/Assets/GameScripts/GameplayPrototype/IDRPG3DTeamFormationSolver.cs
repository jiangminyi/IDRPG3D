using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public readonly struct IDRPG3DFormationMember
    {
        public IDRPG3DFormationMember(int unitId, int teamOrder, int movePriority, float radius)
        {
            UnitId = unitId;
            TeamOrder = teamOrder;
            MovePriority = movePriority;
            Radius = radius;
        }

        public int UnitId { get; }
        public int TeamOrder { get; }
        public int MovePriority { get; }
        public float Radius { get; }
    }

    public readonly struct IDRPG3DFormationDestination
    {
        public IDRPG3DFormationDestination(int unitId, Vector3 worldPosition)
        {
            UnitId = unitId;
            WorldPosition = worldPosition;
        }

        public int UnitId { get; }
        public Vector3 WorldPosition { get; }
    }

    public static class IDRPG3DTeamFormationSolver
    {
        private static readonly List<IDRPG3DFormationMember> SortedMembers = new List<IDRPG3DFormationMember>(16);

        public static void BuildFormation(
            IReadOnlyList<IDRPG3DFormationMember> members,
            Vector3 anchorPosition,
            Vector3 forward,
            float sideSpacing,
            float rowSpacing,
            List<IDRPG3DFormationDestination> destinations)
        {
            destinations.Clear();
            if (members == null || members.Count == 0)
            {
                return;
            }

            SortedMembers.Clear();
            for (var i = 0; i < members.Count; i++)
            {
                SortedMembers.Add(members[i]);
            }

            SortedMembers.Sort(CompareMembers);

            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            var right = new Vector3(forward.z, 0f, -forward.x);
            if (right.sqrMagnitude < 0.0001f)
            {
                right = Vector3.right;
            }
            else
            {
                right.Normalize();
            }

            for (var i = 0; i < SortedMembers.Count; i++)
            {
                var slot = CalculateSlotOffset(i, sideSpacing, rowSpacing);
                var position = anchorPosition + forward * slot.y + right * slot.x;
                destinations.Add(new IDRPG3DFormationDestination(SortedMembers[i].UnitId, position));
            }
        }

        private static int CompareMembers(IDRPG3DFormationMember left, IDRPG3DFormationMember right)
        {
            var priorityComparison = right.MovePriority.CompareTo(left.MovePriority);
            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            return left.TeamOrder.CompareTo(right.TeamOrder);
        }

        private static Vector2 CalculateSlotOffset(int sortedIndex, float sideSpacing, float rowSpacing)
        {
            if (sortedIndex == 0)
            {
                return new Vector2(0f, rowSpacing);
            }

            var row = (sortedIndex + 1) / 2;
            var side = sortedIndex % 2 == 1 ? -1f : 1f;
            var x = side * row * sideSpacing;
            var y = -row * rowSpacing;
            return new Vector2(x, y);
        }
    }
}
