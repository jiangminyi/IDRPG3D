using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	/// <summary> This class is base for any unity ability. To create new ability - derive from it your new classes. To add this ability to unit - add your new component to the unit prefab. </summary>
	public class Ability : MonoBehaviour
	{
		public Unit unitOwner { get; protected set; }
		public AbilitiesModule unitOwnerAbilities { get; protected set; }
		public bool isActive { get; set; }

		[SerializeField] AbilityData data;

		public AbilityData Data { get { return data; } }

		UI.UnitAbilities abilitiesListUI; // todo move from here

		void Awake()
		{
			unitOwner = GetComponent<Unit>();

			if (!data)
			{
				enabled = false;
				return;
			}

			isActive = data.isActiveByDefault;
		}

		void Start()
		{
			unitOwnerAbilities = unitOwner.GetModule<AbilitiesModule>();

			if (!unitOwnerAbilities)
				unitOwnerAbilities = gameObject.AddComponent<AbilitiesModule>();

			abilitiesListUI = FindObjectOfType<UI.UnitAbilities>(); // todo move from here
		}

		/// <summary> This method launches ability action.</summary>
		public void DoAction()
		{
			if (!CanUse())
				return;

			CustomAction();

			if (data.soundToPlayOnUse)
				unitOwner.PlayCustomSound(data.soundToPlayOnUse);

			abilitiesListUI.Redraw(); // todo move from here
		}

		/// <summary> This method can be overrided to add custom ability action. </summary>
		public virtual void CustomAction() { }

		public bool CanUse() { return true; }
	}
}