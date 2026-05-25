using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(IDRPG3DCombatUnit))]
    public sealed class IDRPG3DSelectableUnit : MonoBehaviour
    {
        private IDRPG3DCombatUnit unit;

        private void Awake()
        {
            unit = GetComponent<IDRPG3DCombatUnit>();
        }

        private void OnMouseDown()
        {
            if (unit == null)
            {
                unit = GetComponent<IDRPG3DCombatUnit>();
            }

            Debug.Log($"[IDRPG3D Select] {name} selected. HP {unit.Health:0}/{unit.MaxHealth:0}, priority {unit.MovePriority}.");
        }
    }
}
