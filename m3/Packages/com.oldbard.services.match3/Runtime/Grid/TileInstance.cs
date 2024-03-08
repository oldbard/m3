using OldBard.Services.Match3.Grid.View;
using UnityEngine;

namespace OldBard.Services.Match3.Grid
{
	/// <summary>
	/// Class which represents a Tile from the grid
	/// </summary>
	public class TileInstance
	{
		public int PosX { get; set; }
		public int PosY { get; set; }
		public bool IsValid { get; set; }
		public TileType TileType { get; private set; }
		public TileView TileView { get; private set; }

		public TileInstance(TileView tileView)
		{
			TileView = tileView;
		}

		public void Configure(int x, int y, float width, float height, int yCascadeOffset, Vector3 gridOffset)
		{
			PosX = x;
			PosY = y;

			TileView.SetPosition(x, y, width, height, yCascadeOffset, gridOffset);
		}

		public void SetTileType(TileType tileType)
		{
			TileType = tileType;
			TileView.ChangeTileType(TileType);
		}

		public override string ToString()
		{
			return $"({PosX}, {PosY}), {TileType}";
		}
	}
}
