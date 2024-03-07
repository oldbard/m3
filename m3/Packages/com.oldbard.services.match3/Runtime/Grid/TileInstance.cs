using OldBard.Services.Match3.Grid.Views;

namespace OldBard.Services.Match3.Grid
{
	public class TileInstance
	{
		public int PosX;
		public int PosY;
		public bool Valid;
		public bool Spawned;

		public TileType TileType;
		public TileView TileView;
		
		public override string ToString()
		{
			return $"({PosX}, {PosY}), {TileType}, {Valid}";
		}
	}
}
