using OldBard.Services.Match3.Grid.Data;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OldBard.Services.Match3.Grid.Views
{
    /// <summary>
    /// TilesView. Used to do set and modify the view of the tiles
    /// </summary>
    public class TileView : MonoBehaviour
    {
        const string ANIM_ACTIVE_PARAM = "Blink";
        
        [SerializeField] SpriteRenderer _body;
        [SerializeField] SpriteRenderer _eye;
        [SerializeField] SpriteRenderer _mouth;
        [SerializeField] SpriteRenderer _shadow;
        [SerializeField] SpriteRenderer _selected;
        [SerializeField] Animator _animator;

        Transform _transform;
        Vector3 _defaultScale;

        public TileType AppliedTileType { get; private set; }
        public Vector3 TargetPosition { get; set; }
        TileObject TileObject { get; set; }
        
        static readonly int s_blink = Animator.StringToHash(ANIM_ACTIVE_PARAM);

        public Vector3 Position
        {
            get => _transform.localPosition;
            set => _transform.localPosition = value;
        }

        public Vector3 LocalScale
        {
            get => _transform.localScale;
            set => _transform.localScale = value;
        }

        public bool NeedsRefresh => AppliedTileType != TileObject.TileType;

        void Awake()
        {
            _transform = transform;
            _defaultScale = _transform.localScale;

            _animator.speed = Random.Range(0.8f, 1f);
        }

        public void Init(TileObject tile, TileViewData viewData)
        {
            _body.sprite = viewData.Body;
            _eye.sprite = viewData.Eye;
            _mouth.sprite = viewData.Mouth;
            _shadow.sprite = viewData.Shadow;
            _selected.sprite = viewData.Selected;
            _animator.runtimeAnimatorController = viewData.Animation;

            transform.localScale = _defaultScale;

            TileObject = tile;
            AppliedTileType = tile.TileType;

            DoRandomBlinkAnimation();
        }

        public void HighlightTile()
        {
            _selected.gameObject.SetActive(true);
        }

        public void DisableTileHighlight()
        {
            _selected.gameObject.SetActive(false);
        }

        public void DoBlink()
        {
            _animator.SetTrigger(s_blink);
        }

        public void SetSelectedBackgroundAlpha(float alpha)
        {
            _selected.color = new Color(_selected.color.r, _selected.color.g, _selected.color.b, alpha);
        }

        async void DoRandomBlinkAnimation()
        {
            while(true)
            {
                var timeToBlink = Random.Range(5, 30);
                
                await Task.Delay(timeToBlink * 1000);

                if(_animator == null)
                {
                    return;
                }

                _animator.SetTrigger(s_blink);
            }
        }
    }
}
