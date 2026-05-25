using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[System.Serializable]
	public class ColoredRenderer
	{
		public bool usesHouseColorShader = false;
		[Range(0, 10)] public int materialId;
		public Renderer renderer;

		public void SetMaterial(Material newMaterial)
		{
			Material[] materials = renderer.materials;
			materials[materialId] = newMaterial;
			renderer.materials = materials;
		}

		public void SetColor(Color color)
		{
			Material[] materials = renderer.materials;
			materials[materialId].SetColor("_HouseColor", color);
			renderer.materials = materials;
		}
	}
}