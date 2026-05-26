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
        [SerializeField] private float healthRegenPerSecond = 0f;
        [SerializeField] private bool isBoss;

        private IDRPG3DAnimatorBridge animatorBridge;
        private IDRPG3DPrototypeBuffController buffController;

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
        public float HealthRegenPerSecond => healthRegenPerSecond;
        public bool IsBoss => isBoss;
        public bool IsControlImmune => isBoss;
        public float BonusArmor
        {
            get
            {
                if (buffController == null)
                {
                    buffController = GetComponent<IDRPG3DPrototypeBuffController>();
                }

                return buffController != null ? buffController.ArmorBonus : 0f;
            }
        }
        public bool IsAlive => Health > 0f;
        public IDRPG3DThreatTable<IDRPG3DCombatUnit> ThreatTable { get; } = new IDRPG3DThreatTable<IDRPG3DCombatUnit>();

        public event Action<IDRPG3DCombatUnit, IDRPG3DCombatUnit> Damaged;
        public event Action<IDRPG3DCombatUnit, IDRPG3DCombatUnit> HealthChanged;
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
            float aggro,
            float regenPerSecond = 0f)
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
            healthRegenPerSecond = Mathf.Max(0f, regenPerSecond);
            Initialize();
        }

        public void SetBoss(bool value)
        {
            isBoss = value;
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            TickForTest(Time.deltaTime);
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
            buffController = GetComponent<IDRPG3DPrototypeBuffController>();

            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                Radius = agent.radius;
            }
        }

        public void TickForTest(float deltaTime)
        {
            if (deltaTime <= 0f || healthRegenPerSecond <= 0f || !IsAlive || Health >= maxHealth)
            {
                return;
            }

            HealInternal(healthRegenPerSecond * deltaTime, null, false);
        }

        public void TakeDamage(float amount, IDRPG3DCombatUnit attacker)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            var finalAmount = Mathf.Max(1f, amount - BonusArmor);
            Health = Mathf.Max(0f, Health - finalAmount);
            if (attacker != null)
            {
                ThreatTable.AddThreat(attacker, finalAmount);
            }

            Debug.Log($"[IDRPG3D Combat] {name} took {finalAmount:0.#} damage from {(attacker != null ? attacker.name : "unknown")}. HP {Health:0.#}/{MaxHealth:0.#}.");
            Damaged?.Invoke(this, attacker);
            HealthChanged?.Invoke(this, attacker);

            if (Health <= 0f)
            {
                Die();
            }
        }

        public float Heal(float amount, IDRPG3DCombatUnit healer)
        {
            return HealInternal(amount, healer, true);
        }

        private float HealInternal(float amount, IDRPG3DCombatUnit healer, bool log)
        {
            if (!IsAlive || amount <= 0f)
            {
                return 0f;
            }

            var before = Health;
            Health = Mathf.Min(maxHealth, Health + amount);
            var healed = Health - before;
            if (healed > 0f)
            {
                if (log)
                {
                    Debug.Log($"[IDRPG3D Combat] {name} healed {healed:0.#} from {(healer != null ? healer.name : "unknown")}. HP {Health:0.#}/{MaxHealth:0.#}.");
                }

                HealthChanged?.Invoke(this, healer);
            }

            return healed;
        }

        private void Die()
        {
            Debug.Log($"[IDRPG3D Combat] {name} died.");
            animatorBridge?.SetDead(true);
            Died?.Invoke(this);
        }
    }
}
