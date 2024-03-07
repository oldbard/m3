using OldBard.Services.Match3.Grid.Data;
using OldBard.Services.Match3.Grid.Views;
using UnityEngine;
using UnityEngine.Pool;

namespace OldBard.Services.Match3.Grid
{
	public class TileInstanceFactory
	{
		const string GRID_HEIGHT_KEY = "GridHeight";

		static TileInstanceFactory s_instance;
		
		public static TileInstanceFactory Instance => s_instance ??= new TileInstanceFactory();
		
		GridConfig _config;
		GridSettings _gridSettings;
		int _variation;
		int _gridHeight;

		Transform _poolParentTransform;
		
		IObjectPool<TileInstance> _tilesPool;

		public void Initialize(GridConfig config, GridSettings gridSettings, Transform poolParentTransform, int variation, int initialSize)
		{
			_config = config;
			_gridSettings = gridSettings;
			_variation = variation;

			_gridHeight = PlayerPrefs.GetInt(GRID_HEIGHT_KEY, _gridSettings.DefaultGridHeight);

			_poolParentTransform = poolParentTransform;

			_tilesPool = new ObjectPool<TileInstance>(OnCreateTileInstance, OnGetTileInstance, OnReleaseTileInstance, OnDestroyTileInstance, false, initialSize);
		}

		public void Terminate()
		{
			_tilesPool.Clear();
			_tilesPool = null;
		}

		public TileInstance GetNewTile(int x, int y, Vector3 gridOffset)
		{
			TileInstance tileInstance = GetTileInstance();
			tileInstance.Configure(x, y, _gridSettings.TileViewWidth, _gridSettings.TileViewHeight,
				_gridHeight + _gridSettings.YCascadePositionOffset, gridOffset);

			return tileInstance;
		}

		TileInstance GetTileInstance()
		{
			TileInstance tileInstance = _tilesPool.Get();
			tileInstance.TileView.Initialize(_config, _variation);

			return tileInstance;
		}

		public void ReleaseTileInstance(TileInstance tileInstance)
		{
			_tilesPool.Release(tileInstance);
		}

		TileInstance OnCreateTileInstance()
		{
			TileView tileView = Object.Instantiate(_config.TilePrefab, _poolParentTransform.transform);
			tileView.Initialize(_config, _variation);
			
			var tileInstance = new TileInstance(tileView);

			return tileInstance;
		}

		void OnGetTileInstance(TileInstance tileInstance)
		{
			tileInstance.IsValid = true;
			tileInstance.TileView.Activate();
		}

		void OnReleaseTileInstance(TileInstance tileInstance)
		{
			tileInstance.TileView.Deactivate();
		}

		void OnDestroyTileInstance(TileInstance tileInstance)
		{
			Object.Destroy(tileInstance.TileView.gameObject);
		}
	}
}
