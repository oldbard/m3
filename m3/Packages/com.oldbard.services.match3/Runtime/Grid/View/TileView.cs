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
        const int TILE_Z_POS = -2;

        const string ANIM_ACTIVE_PARAM = "Blink";
        
        static readonly int s_blink = Animator.StringToHash(ANIM_ACTIVE_PARAM);

        [SerializeField] SpriteRenderer _body;
        [SerializeField] SpriteRenderer _eye;
        [SerializeField] SpriteRenderer _mouth;
        [SerializeField] SpriteRenderer _shadow;
        [SerializeField] SpriteRenderer _selected;
        [SerializeField] Animator _animator;

        Transform _transform;
        Vector3 _defaultScale = Vector3.one;

        public TileType AppliedTileType { get; private set; }
        public Vector3 TargetPosition { get; set; }

        public Vector3 Position
        {
            get => _transform.localPosition;
            set => _transform.localPosition = value;
        }

        public Vector3 LocalScale
        {
            set => _transform.localScale = value;
        }

        GridConfig _config;

        int _variation;

        void Awake()
        {
            _transform = transform;
            _defaultScale = _transform.localScale;

            _animator.speed = Random.Range(0.8f, 1f);
        }

        public void Initialize(GridConfig config, int variation)
        {
            _config = config;
            
            _variation = variation;
        }

        public void ChangeTileType(TileType tileType)
        {
            TileViewData viewData = _config.GetViewData(tileType, _variation);
            
            _body.sprite = viewData.Body;
            _eye.sprite = viewData.Eye;
            _mouth.sprite = viewData.Mouth;
            _shadow.sprite = viewData.Shadow;
            _selected.sprite = viewData.Selected;
            _animator.runtimeAnimatorController = viewData.Animation;

            transform.localScale = _defaultScale;

            name = tileType.ToString();

            AppliedTileType = tileType;

            DoRandomBlinkAnimation();
        }

        public void SetPosition(int x, int y, float width, float height, int yCascadeOffset, Vector3 gridOffset)
        {
            var localPosition = new Vector3(
                x * width,
                (y * height) + yCascadeOffset,
                TILE_Z_POS);
            
            localPosition += gridOffset;
            transform.localPosition = localPosition;

            localPosition.y -= yCascadeOffset;

            TargetPosition = localPosition;
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
            Color color = _selected.color;
            color = new Color(color.r, color.g, color.b, alpha);
            
            _selected.color = color;
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

        public void Activate()
        {
            transform.localScale = _defaultScale;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            AppliedTileType = TileType.None;
        }
    }
}
