using UnityEngine;
using System.Collections;

namespace InsaneSystems.RTSStarterKit
{
	public class RadarRenderWorker : MonoBehaviour
	{
		[SerializeField] bool isFOWRadarCamera;
		
		bool isPreparedToWork;
		Camera radarCamera;

		Texture2D fowRenderedTexture;
		RenderTexture FoWRT;
		
		int resolution = 512;

		public void Start()
		{
			SetupCorrectParameters();
		}

		void SetupCorrectParameters()
		{
			var renderLight = GetComponent<Light>();

			if (renderLight)
				renderLight.enabled = true;

			float mapSize = MatchSettings.currentMatchSettings.selectedMap.mapSize;

			radarCamera = GetComponent<Camera>();

			radarCamera.orthographicSize = mapSize / 2f;
			radarCamera.transform.position = new Vector3(mapSize / 2f, 64, mapSize / 2f);
			radarCamera.aspect = 1.0f;
			
			RenderTexture tempRT = new RenderTexture(resolution, resolution, 24);
			
			radarCamera.targetTexture = tempRT;
			//radarCamera.Render();

			RenderTexture.active = tempRT;

			isPreparedToWork = true;

			/*
			if (isFOWRadarCamera)
			{
				FoWRT = new RenderTexture((int)mapSize / 4, (int)mapSize / 4, 24);
				radarCamera.targetTexture = FoWRT;
				RenderTexture.active = FoWRT;
				
				fowRenderedTexture = new Texture2D((int)mapSize / 4, (int)mapSize / 4, TextureFormat.RGB24, false);
				UI.UIController.instance.minimapComponent.SetFowTexture(fowRenderedTexture);
			}
			*/
		}

		void OnPostRender()
		{
			if (!isPreparedToWork)
				return;

			if (!isFOWRadarCamera)
				MapRender();
			else
				FowMapRender();
		}

		void FowMapRender()
		{
			/*
			RenderTexture.active = FoWRT;
			
			fowRenderedTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
			fowRenderedTexture.Apply();
			UI.UIController.instance.minimapComponent.SetMapBackground(fowRenderedTexture);
			
			RenderTexture.active = null;
			*/
		}
		
		void MapRender()
		{
			Texture2D renderedTexture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);

			renderedTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
			renderedTexture.Apply();

			RenderTexture.active = null;

			UI.UIController.instance.minimapComponent.SetMapBackground(renderedTexture);
			
			Destroy(gameObject);
		}
	}

}