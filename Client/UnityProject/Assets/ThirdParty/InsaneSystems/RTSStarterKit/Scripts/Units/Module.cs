using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Module : MonoBehaviour
	{
		public Unit selfUnit { get; protected set; }

		protected virtual void Awake()
		{
			selfUnit = GetComponent<Unit>();
			selfUnit.RegisterModule(this);
		}
	}
}