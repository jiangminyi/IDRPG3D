using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class AddMoneyTrigger : TriggerBase
	{
		[Range(0, 15)] public int playerToAddMoney = 0;
		public int moneyToAdd;

		protected override void ExecuteAction()
		{
			Player.GetPlayerById((byte)playerToAddMoney).AddMoney(moneyToAdd);
		}
	}
}