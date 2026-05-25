
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public class InputHandler : MonoBehaviour
	{
		/// <summary> Contains current player world cursor hit point, getted by ScreenPointToRay method. </summary>
		public static RaycastHit currentCursorWorldHit;

		Camera mainCamera;
		
		void Start()
		{
			mainCamera = Camera.main;
			
			Selection.InitializeHotkeys(); // todo invert it all to keymapInitializeEvent
			FindObjectOfType<CameraMover>().InitializeHotkeys();
		}

		void Update()
		{
			HandleSelectionInput();
			HandleOrdersInput();
			HandleWorldCursorPosition();

			Keymap.loadedKeymap.CheckAllKeys();
		}

		void HandleWorldCursorPosition()
		{
			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 10000))
				currentCursorWorldHit = hit;
		}

		void HandleSelectionInput()
		{
			if (Build.isBuildMode)
				return;

			if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				Selection.startMousePosition = Input.mousePosition;

				Selection.OnStartSelection();
			}

			if (Input.GetMouseButtonUp(0))
			{
				Selection.endMousePosition = Input.mousePosition;

				if (IsJustClick(Selection.startMousePosition, Selection.endMousePosition) && !EventSystem.current.IsPointerOverGameObject())
					Selection.OnSingleSelection();
				else if (Selection.isSelectionStarted)
					Selection.OnEndSelection();
			}
		}

		static bool IsJustClick(Vector2 positionA, Vector2 positionB) { return Vector2.Distance(positionA, positionB) < 5f; }

		void HandleOrdersInput()
		{
			if (Selection.selectedUnits.Count == 0)
				return;

			if (Input.GetMouseButtonUp(1))
				Ordering.GiveOrder(Input.mousePosition, Input.GetKey(KeyCode.LeftShift));
		}
	}
}