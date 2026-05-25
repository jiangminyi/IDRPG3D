using System;
using System.Collections.Generic;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DThreatTable<TTarget> where TTarget : class
    {
        private readonly Dictionary<TTarget, float> threatByTarget = new Dictionary<TTarget, float>();

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
            target = null;
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

            return target != null;
        }
    }
}
