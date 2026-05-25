using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class Cursors
	{
		public static void SetDefaultCursor()
		{
			SetCursor(GameController.instance.MainStorage.defaultCursour);
		}
		
		public static void SetAttackCursor()
		{
			SetCursor(GameController.instance.MainStorage.attackCursour, new Vector2(0.5f, 0.5f));
		}
		
		public static void SetRestrictCursor()
		{
			SetCursor(GameController.instance.MainStorage.restrictCursour, new Vector2(0.5f, 0.5f));
		}

		public static void SetResourcesCursor()
		{
			SetCursor(GameController.instance.MainStorage.gatherResourcesCursour, new Vector2(0.5f, 0.5f));
		}
		
		public static void SetGiveResourcesCursor()
		{
			SetCursor(GameController.instance.MainStorage.giveResourcesCursour, new Vector2(0.5f, 0.5f));
		}
		
		public static void SetMapOrderCursor()
		{
			SetCursor(GameController.instance.MainStorage.mapOrderCursor);
		}
		
		public static void SetCursor(Texture2D cursorTexture)
		{
			SetCursor(cursorTexture, Vector2.up);
		}
		
		public static void SetCursor(Texture2D cursorTexture, Vector2 hotSpot)
		{
			Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
		}
	}
}