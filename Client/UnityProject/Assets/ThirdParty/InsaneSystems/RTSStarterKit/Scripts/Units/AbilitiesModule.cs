using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InsaneSystems.RTSStarterKit.Abilities;

namespace InsaneSystems.RTSStarterKit
{
	public class AbilitiesModule : Module
	{
		public List<Ability> abilities { get; protected set; }

		void Start()
		{
			abilities = GetComponents<Ability>().ToList();
		}

		public Ability GetAbility(AbilityData abilityData)
		{
			for (int i = 0; i < abilities.Count; i++)
			{
				if (abilities[i].Data == abilityData)
					return abilities[i];
			}

			return null;
		}

		public Ability GetAbilityById(int id)
		{
			if (abilities.Count > id)
				return abilities[id];
			
			return null;
		}
	}
}