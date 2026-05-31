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
        [SerializeField] private int level = 1;
        [SerializeField] private int experienceReward;

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
        public int Level => level;
        public int ExperienceReward => experienceReward;
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
        public event Action<IDRPG3DCombatFloatingTextEvent> FloatingTextRequested;
        public event Action<IDRPG3DCombatUnit> Died;
        public event Action<IDRPG3DCombatUnit> LevelChanged;

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

        public void SetLevel(int value)
        {
            var nextLevel = Mathf.Max(1, value);
            if (level == nextLevel)
            {
                return;
            }

            level = nextLevel;
            LevelChanged?.Invoke(this);
        }

        public void SetExperienceReward(int value)
        {
            experienceReward = Mathf.Max(0, value);
        }

        public void SetBoss(bool value)
        {
            isBoss = value;
        }

        public void SetBaseStats(
            float health,
            float damage,
            float range,
            float interval,
            float aggro,
            float regenPerSecond,
            bool keepHealthRatio)
        {
            var healthRatio = maxHealth > 0f ? Mathf.Clamp01(Health / maxHealth) : 1f;
            maxHealth = Mathf.Max(1f, health);
            attackPower = Mathf.Max(0f, damage);
            attackRange = Mathf.Max(0.1f, range);
            attackInterval = Mathf.Max(0.1f, interval);
            aggroRadius = Mathf.Max(0.1f, aggro);
            healthRegenPerSecond = Mathf.Max(0f, regenPerSecond);
            Health = keepHealthRatio ? maxHealth * healthRatio : maxHealth;
            HealthChanged?.Invoke(this, null);
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
                IDRPG3DPrototypeCombatDirector.ShareThreatWithNearbyAllies(this, attacker, finalAmount);
            }

            IDRPG3DPrototypeDebugLog.Combat($"[IDRPG3D Combat] {name} took {finalAmount:0.#} damage from {(attacker != null ? attacker.name : "unknown")}. HP {Health:0.#}/{MaxHealth:0.#}.");
            Damaged?.Invoke(this, attacker);
            FloatingTextRequested?.Invoke(IDRPG3DCombatFloatingTextEvent.Damage(this, attacker, finalAmount));
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

        public bool Revive(float healthRatio)
        {
            if (IsAlive)
            {
                return false;
            }

            Health = Mathf.Clamp01(healthRatio) * maxHealth;
            if (Health <= 0f)
            {
                Health = Mathf.Min(1f, maxHealth);
            }

            animatorBridge?.SetDead(false);
            HealthChanged?.Invoke(this, null);
            return true;
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
                    IDRPG3DPrototypeDebugLog.Combat($"[IDRPG3D Combat] {name} healed {healed:0.#} from {(healer != null ? healer.name : "unknown")}. HP {Health:0.#}/{MaxHealth:0.#}.");
                }

                if (log)
                {
                    FloatingTextRequested?.Invoke(IDRPG3DCombatFloatingTextEvent.Heal(this, healer, healed));
                }

                HealthChanged?.Invoke(this, healer);
            }

            return healed;
        }

        private void Die()
        {
            IDRPG3DPrototypeDebugLog.Combat($"[IDRPG3D Combat] {name} died.");
            animatorBridge?.SetDead(true);
            Died?.Invoke(this);
        }
    }

    public enum IDRPG3DCombatFloatingTextValueType
    {
        None,
        Damage,
        Heal
    }

    public enum IDRPG3DCombatFloatingTextKind
    {
        None,
        NormalDamage,
        CriticalDamage,
        Heal
    }

    public readonly struct IDRPG3DCombatFloatingTextEvent
    {
        private IDRPG3DCombatFloatingTextEvent(
            IDRPG3DCombatFloatingTextValueType valueType,
            IDRPG3DCombatUnit target,
            IDRPG3DCombatUnit source,
            float value,
            bool isCritical)
        {
            ValueType = valueType;
            Target = target;
            Source = source;
            Value = Mathf.Max(0f, value);
            IsCritical = isCritical;
        }

        public IDRPG3DCombatFloatingTextValueType ValueType { get; }
        public IDRPG3DCombatUnit Target { get; }
        public IDRPG3DCombatUnit Source { get; }
        public float Value { get; }
        public bool IsCritical { get; }

        public IDRPG3DCombatFloatingTextKind Kind => IDRPG3DCombatFloatingTextClassifier.Classify(ValueType, Value, IsCritical);

        public static IDRPG3DCombatFloatingTextEvent Damage(
            IDRPG3DCombatUnit target,
            IDRPG3DCombatUnit source,
            float value,
            bool isCritical = false)
        {
            return new IDRPG3DCombatFloatingTextEvent(
                IDRPG3DCombatFloatingTextValueType.Damage,
                target,
                source,
                value,
                isCritical);
        }

        public static IDRPG3DCombatFloatingTextEvent Heal(
            IDRPG3DCombatUnit target,
            IDRPG3DCombatUnit source,
            float value)
        {
            return new IDRPG3DCombatFloatingTextEvent(
                IDRPG3DCombatFloatingTextValueType.Heal,
                target,
                source,
                value,
                isCritical: false);
        }
    }

    public static class IDRPG3DCombatFloatingTextClassifier
    {
        public static IDRPG3DCombatFloatingTextKind Classify(
            IDRPG3DCombatFloatingTextValueType valueType,
            float value,
            bool isCritical = false)
        {
            if (value <= 0f)
            {
                return IDRPG3DCombatFloatingTextKind.None;
            }

            if (valueType == IDRPG3DCombatFloatingTextValueType.Heal)
            {
                return IDRPG3DCombatFloatingTextKind.Heal;
            }

            if (valueType == IDRPG3DCombatFloatingTextValueType.Damage)
            {
                return isCritical
                    ? IDRPG3DCombatFloatingTextKind.CriticalDamage
                    : IDRPG3DCombatFloatingTextKind.NormalDamage;
            }

            return IDRPG3DCombatFloatingTextKind.None;
        }
    }
}
