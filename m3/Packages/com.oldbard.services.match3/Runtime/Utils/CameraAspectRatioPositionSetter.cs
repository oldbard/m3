using UnityEngine;

namespace OldBard.Services.Match3.Utils
{
	[RequireComponent(typeof(Camera))]
	public class CameraAspectRatioPositionSetter : MonoBehaviour
	{
		[SerializeField] Camera _camera;
		[SerializeField] Vector2 _referenceResolution;
		[SerializeField] float _referenceXPosition;

		float _referenceRatio;

		void Awake()
		{
			_referenceRatio = GetReferenceRation();

			UpdatePosition();
		}

		void UpdatePosition()
		{
			Transform trans = transform;

			Vector3 currentPosition = trans.localPosition;

			trans.localPosition = new Vector3(GetXPosition(), currentPosition.y, currentPosition.z);
		}

		float GetXPosition()
		{
			var ratio = _camera.aspect / _referenceRatio;

			return _referenceXPosition * ratio;
		}

		float GetReferenceRation()
		{
			return _referenceResolution.x / _referenceResolution.y;
		}

#if UNITY_EDITOR
		public void UpdateEditorPosition()
		{
			_referenceRatio = GetReferenceRation();

			UpdatePosition();
		}

		public float GetAspectRatio()
		{
			return _camera.aspect;
		}

		public float GetCalculatedX()
		{
			return GetXPosition();
		}
#endif
	}
}
