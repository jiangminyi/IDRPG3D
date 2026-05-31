using System;
using System.Collections.Generic;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DThreatTable<TTarget> where TTarget : class
    {
        private readonly Dictionary<TTarget, float> threatByTarget = new Dictionary<TTarget, float>();

        public bool HasAnyThreat => threatByTarget.Count > 0;

        public void AddThreat(TTarget target, float amount)
        {
            if (target == null || amount <= 0f)
            {
                return;
            }

            threatByTarget.TryGetValue(target, out var current);
            threatByTarget[target] = current + amount;
        }

        public void Clear()
        {
            threatByTarget.Clear();
        }

        public bool TryGetHighestThreatTarget(Predicate<TTarget> isValid, out TTarget target)
        {
            return TryGetHighestThreatTarget(isValid, out target, out _);
        }

        public bool TryGetHighestThreatTarget(Predicate<TTarget> isValid, out TTarget target, out float threat)
        {
            target = null;
            threat = 0f;
            var bestThreat = float.MinValue;

            foreach (var pair in threatByTarget)
            {
                if (pair.Key == null || (isValid != null && !isValid(pair.Key)))
                {
                    continue;
                }

                if (target == null || pair.Value > bestThreat)
                {
                    target = pair.Key;
                    bestThreat = pair.Value;
                }
            }

            threat = target != null ? bestThreat : 0f;
            return target != null;
        }

        public float GetThreat(TTarget target)
        {
            return target != null && threatByTarget.TryGetValue(target, out var threat) ? threat : 0f;
        }
    }
}
