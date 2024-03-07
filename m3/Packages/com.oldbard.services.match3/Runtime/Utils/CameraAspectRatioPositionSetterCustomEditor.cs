using UnityEditor;
using UnityEngine;

namespace OldBard.Services.Utils
{
	[CustomEditor(typeof(CameraAspectRatioPositionSetter))]
	public class CameraAspectRatioPositionSetterCustomEditor : Editor
	{
		CameraAspectRatioPositionSetter _cameraAspectRatioPositionSetter;
		
		protected void OnEnable()
		{
			_cameraAspectRatioPositionSetter = (CameraAspectRatioPositionSetter) target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			GUILayout.BeginVertical();

			GUILayout.Label($"Current Aspect Ratio: {_cameraAspectRatioPositionSetter.GetAspectRatio()}");
			GUILayout.Label($"Calculated X: {_cameraAspectRatioPositionSetter.GetCalculatedX()}");

			if(GUILayout.Button("Refresh Position with Landscape"))
			{
				_cameraAspectRatioPositionSetter.UpdateEditorPosition();
			}
			
			GUILayout.EndVertical();
		}
	}
}
