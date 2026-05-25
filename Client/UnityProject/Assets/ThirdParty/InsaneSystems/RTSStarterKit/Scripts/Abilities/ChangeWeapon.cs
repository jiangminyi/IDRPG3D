namespace InsaneSystems.RTSStarterKit.Abilities
{
	public class ChangeWeapon : Ability
	{
		public override void CustomAction()
		{
			isActive = false;

			var attackable = unitOwner.GetModule<Attackable>();

			if (attackable)
			{
				attackable.customAttackDistance = Data.newAttackRange;
				attackable.customDamage = Data.newAttackDamage;
				attackable.customReloadTime = Data.newAttackReloadTime;
			}

			unitOwnerAbilities.GetAbility(Data.customWeaponAbilityToEnable).isActive = true;
		}
	}
}