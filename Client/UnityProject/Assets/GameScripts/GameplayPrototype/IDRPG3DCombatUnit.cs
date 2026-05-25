using System;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DCombatUnit : MonoBehaviour
    {
        [SerializeField] private int unitId;
        [SerializeField] private int teamOrder;
        [SerializeField] private IDRPG3DCombatFaction faction;
        [SerializeField] private int movePriority = 10;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float attackPower = 12f;
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private float attackInterval = 1.4f;
        [SerializeField] private float aggroRadius = 7f;

        private IDRPG3DAnimatorBridge animatorBridge;

        public int UnitId => unitId;
        public int TeamOrder => teamOrder;
        public IDRPG3DCombatFaction Faction => faction;
        public int MovePriority => movePriority;
        public float Radius { get; private set; } = 0.35f;
        public float MaxHealth => maxHealth;
        public float Health { get; private set; }
        public float AttackPower => attackPower;
        public float AttackRange => attackRange;
        public float AttackInterval => attackInterval;
        public float AggroRadius => aggroRadius;
        public bool IsAlive => Health > 0f;
        public IDRPG3DThreatTable<IDRPG3DCombatUnit> ThreatTable { get; } = new IDRPG3DThreatTable<IDRPG3DCombatUnit>();

        public event Action<IDRPG3DCombatUnit, IDRPG3DCombatUnit> Damaged;
        public event Action<IDRPG3DCombatUnit> Died;

        public void Configure(
            int id,
            int order,
            IDRPG3DCombatFaction unitFaction,
            int priority,
            float health,
            float damage,
            float range,
            float interval,
            float aggro)
        {
            unitId = id;
            teamOrder = order;
            faction = unitFaction;
            movePriority = priority;
            maxHealth = Mathf.Max(1f, health);
            attackPower = Mathf.Max(0f, damage);
            attackRange = Mathf.Max(0.1f, range);
            attackInterval = Mathf.Max(0.1f, interval);
            aggroRadius = Mathf.Max(0.1f, aggro);
            Initialize();
        }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            Health = maxHealth;
            ThreatTable.Clear();

            animatorBridge = GetComponent<IDRPG3DAnimatorBridge>();
            if (animatorBridge == null)
            {
                animatorBridge = gameObject.AddComponent<IDRPG3DAnimatorBridge>();
            }
            animatorBridge.Initialize();
            animatorBridge.SetDead(false);

            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                Radius = agent.radius;
            }
        }

        public void TakeDamage(float amount, IDRPG3DCombatUnit attacker)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - amount);
            if (attacker != null)
            {
                ThreatTable.AddThreat(attacker, amount);
            }

            Debug.Log($"[IDRPG3D Combat] {name} took {amount:0.#} damage from {(attacker != null ? attacker.name : "unknown")}. HP {Health:0.#}/{MaxHealth:0.#}.");
            Damaged?.Invoke(this, attacker);

            if (Health <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"[IDRPG3D Combat] {name} died.");
            animatorBridge?.SetDead(true);
            Died?.Invoke(this);
        }
    }
}
