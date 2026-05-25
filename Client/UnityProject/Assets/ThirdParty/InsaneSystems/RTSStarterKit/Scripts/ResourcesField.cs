using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class ResourcesField : MonoBehaviour
	{
		public void OnMouseEnter()
		{
			if (Selection.selectedUnits.Count == 0 || !Selection.selectedUnits[0].data.isHarvester)
				return;
			
			var selectedHarvester = Selection.selectedUnits[0].GetModule<Harvester>();
			var needResourcesCursour = selectedHarvester.harvestedResources < selectedHarvester.MaxResources;
			
			if (needResourcesCursour)
				Cursors.SetResourcesCursor();
			else
				Cursors.SetRestrictCursor();
		}
		
		public void OnMouseExit()
		{
			Cursors.SetDefaultCursor();
		}
	}
}