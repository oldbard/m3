using UnityEditor;
using UnityEngine;

namespace OldBard.Services.Match3.Utils
{
	[CustomEditor(typeof(OrientationAnchorUpdater))]
	public class OrientationAnchorUpdaterCustomEditor : Editor
	{
		OrientationAnchorUpdater _orientationAnchorUpdater;
		
		protected void OnEnable()
		{
			_orientationAnchorUpdater = (OrientationAnchorUpdater) target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.BeginHorizontal();

			if(GUILayout.Button("Refresh Orientation with Landscape"))
			{
				_orientationAnchorUpdater.ForceRefreshOrientation(ScreenOrientation.LandscapeLeft);
			}
			if(GUILayout.Button("Refresh Orientation with Portrait"))
			{
				_orientationAnchorUpdater.ForceRefreshOrientation(ScreenOrientation.Portrait);
			}

			GUILayout.EndHorizontal();
		}
	}
}
