using InsaneSystems.RTSStarterKit.Controls;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	public class CarryOut : Ability
	{
		public override void CustomAction()
		{
			if (Selection.selectedUnits.Count == 0)
				return;

			for (int i = 0; i < Selection.selectedUnits.Count; i++)
			{
				var carryModule = Selection.selectedUnits[i].GetModule<CarryModule>();

				if (carryModule)
					carryModule.ExitAllUnits();
			}
		}
	}
}