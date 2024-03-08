using OldBard.Services.Match3.Config.Data;
using OldBard.Services.Match3.Grid.Tiles.Data;
using OldBard.Services.Match3.Grid.Tiles.View;
using UnityEngine;
using UnityEngine.Pool;

namespace OldBard.Services.Match3.Grid.Tiles.Factories
{
	/// <summary>
	/// Factory to generate TileInstance objects. Uses Unity's IObjectPool to handle the instances.
	/// </summary>
	public class TileInstanceFactory
	{
		const string GRID_HEIGHT_KEY = "GridHeight";

		static TileInstanceFactory s_instance;
		
		public static TileInstanceFactory Instance => s_instance ??= new TileInstanceFactory();
		
		GameConfig _config;
		GridConfig _gridConfig;
		int _variation;
		int _gridHeight;

		Transform _poolParentTransform;
		
		IObjectPool<TileInstance> _tilesPool;

		/// <summary>
		/// Initialization call to set up some configs and parameters
		/// </summary>
		/// <param name="config">Game Config</param>
		/// <param name="gridConfig">Grid Settings</param>
		/// <param name="poolParentTransform">Transform which will hold the TileInstances</param>
		/// <param name="variation">Chosen tiles visual variation</param>
		/// <param name="initialSize">Pool initial size</param>
		public void Initialize(GameConfig config, GridConfig gridConfig, Transform poolParentTransform, int variation, int initialSize)
		{
			_config = config;
			_gridConfig = gridConfig;
			_variation = variation;

			_gridHeight = PlayerPrefs.GetInt(GRID_HEIGHT_KEY, _gridConfig.DefaultGridHeight);

			_poolParentTransform = poolParentTransform;

			_tilesPool = new ObjectPool<TileInstance>(OnCreateTileInstance, OnGetTileInstance, OnReleaseTileInstance, OnDestroyTileInstance, false, initialSize);
		}

		/// <summary>
		/// Clears the Pool
		/// </summary>
		public void Terminate()
		{
			_tilesPool.Clear();
			_tilesPool = null;
		}

		/// <summary>
		/// Gets a TileInstance instance from the Pool
		/// </summary>
		/// <param name="x">X Position</param>
		/// <param name="y">Y Position</param>
		/// <param name="gridOffset">Grid Offset</param>
		/// <returns></returns>
		public TileInstance GetNewTile(int x, int y, Vector3 gridOffset)
		{
			TileInstance tileInstance = GetTileInstance();
			tileInstance.Configure(x, y, _gridConfig.TileViewWidth, _gridConfig.TileViewHeight,
				_gridHeight + _gridConfig.YCascadePositionOffset, gridOffset);

			return tileInstance;
		}

		/// <summary>
		/// Releases a TileInstance object
		/// </summary>
		/// <param name="tileInstance">The instance to be released into the Pool</param>
		public void ReleaseTileInstance(TileInstance tileInstance)
		{
			_tilesPool.Release(tileInstance);
		}

		TileInstance GetTileInstance()
		{
			TileInstance tileInstance = _tilesPool.Get();
			tileInstance.TileView.Initialize(_config, _variation);

			return tileInstance;
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
