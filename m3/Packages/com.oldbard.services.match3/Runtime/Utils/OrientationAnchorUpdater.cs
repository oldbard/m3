using UnityEngine;

namespace OldBard.Services.Match3.Utils
{
    /// <summary>
    /// Class that changes the anchors based on the orientation
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class OrientationAnchorUpdater : MonoBehaviour
    {
        [SerializeField] Vector2 _landscapePivot = new(0f, 0.5f);
        [SerializeField] Vector2 _landscapeMinMax = new(0f, 0.5f);
        [SerializeField] Vector2 _landscapeOffSet = Vector2.zero;

        [SerializeField] Vector2 _portraitPivot = new(0.5f, 1f);
        [SerializeField] Vector2 _portraitMinMax = new(0.5f, 1f);
        [SerializeField] Vector2 _portraitOffSet = Vector2.zero;

        [SerializeField] RectTransform _rectTransform;

        ScreenOrientation _currentOrientation = 0;

        void Awake()
        {
            UpdateOrientation();
        }

        void OnEnable()
        {
            UpdateOrientation();
        }
        
        void OnRectTransformDimensionsChange()
        {
            UpdateOrientation();
        }

        void UpdateOrientation()
        {
            ScreenOrientation orientation = Screen.orientation;

#if UNITY_EDITOR
            orientation = GetEditorTimeOrientation();
#endif
            if (orientation == _currentOrientation)
            {
                return;
            }

            _currentOrientation = orientation;

            RefreshOrientation(orientation);
        }

        void RefreshOrientation(ScreenOrientation orientation)
        {
            switch(orientation)
            {
                case ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown:
                    _rectTransform.anchorMin = _portraitMinMax;
                    _rectTransform.anchorMax = _portraitMinMax;
                    _rectTransform.pivot = _portraitPivot;
                    _rectTransform.ForceUpdateRectTransforms();

                    _rectTransform.anchoredPosition = _portraitOffSet;
                    break;
                case ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight:
                    _rectTransform.anchorMin = _landscapeMinMax;
                    _rectTransform.anchorMax = _landscapeMinMax;
                    _rectTransform.pivot = _landscapePivot;
                    _rectTransform.ForceUpdateRectTransforms();

                    _rectTransform.anchoredPosition = _landscapeOffSet;
                    break;
            }
        }

#if UNITY_EDITOR
        public void ForceRefreshOrientation(ScreenOrientation orientation)
        {
            RefreshOrientation(orientation);
        }

        ScreenOrientation GetEditorTimeOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        }
#endif
    }
}