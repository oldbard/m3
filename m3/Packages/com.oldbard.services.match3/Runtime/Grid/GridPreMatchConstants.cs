namespace OldBard.Services.Match3.Grid
{
	public static class GridPreMatchConstants
	{
		// M Stands for Match
		
		// XXO

		public static readonly (int[,], int[,]) MatchPatternHXXO = ( MatchPatternHXXORequiredTiles, MatchPatternHXXOMatchTiles );
		public static readonly (int[,], int[,]) MatchPatternHXOX = ( MatchPatternHXOXRequiredTiles, MatchPatternHXOXMatchTiles );
		public static readonly (int[,], int[,]) MatchPatternHOXX = ( MatchPatternHOXXRequiredTiles, MatchPatternHOXXMatchTiles );

		public static readonly (int[,], int[,]) MatchPatternVXXO = ( MatchPatternVXXORequiredTiles, MatchPatternVXXOMatchTiles );
		public static readonly (int[,], int[,]) MatchPatternVXOX = ( MatchPatternVXOXRequiredTiles, MatchPatternVXOXMatchTiles );
		public static readonly (int[,], int[,]) MatchPatternVOXX = ( MatchPatternVOXXRequiredTiles, MatchPatternVOXXMatchTiles );

		// 01
		// XX
		public static readonly int[,] MatchPatternHXXORequiredTiles =
		{
			{ 0, 0 },
			{ 1, 0 }
		};

		//           0123
		//  1 Above  --M
		//  0 Middle XX-M
		// -1 Below  --M
		public static readonly int[,] MatchPatternHXXOMatchTiles =
		{
			{ 2, 1 },
			{ 3, 0 },
			{ 2, -1 }
		};
	
		/// *** ///
		
		// XOX

		// 012
		// X-X
		public static readonly int[,] MatchPatternHXOXRequiredTiles =
		{
			{ 0, 0 },
			{ 2, 0 }
		};

		//          012
		//  1 Above -M-   
		//  0       X0X
		// -1 Below -M-
		public static readonly int[,] MatchPatternHXOXMatchTiles =
		{
			{ 1, 1 },
			{ 1, -1 }
		};
	
		/// *** ///
		
		// OXX

		// -XX
		public static readonly int[,] MatchPatternHOXXRequiredTiles =
		{
			{ 1, 0 },
			{ 2, 0 }
		};

		//       -1012
		// Above  -M--
		// Left   M-XX
		// Below  -M--
		public static readonly int[,] MatchPatternHOXXMatchTiles =
		{
			{ 0, 1 },
			{ -1, 0 },
			{ 0, -1 }
		};
	
		/// *** ///
		
		// O
		// X
		// X

		// XX
		public static readonly int[,] MatchPatternVXXORequiredTiles =
		{
			{ 0, 0 },
			{ 0, 1 }
		};

		//   Left - Middle - Right
		//  -101    0        01
		// 3        M
		// 2 M-     O        -M
		// 1  X     X        X
		// 0  X     X        X
		public static readonly int[,] MatchPatternVXXOMatchTiles =
		{
			{ -1, 2 },
			{ 0, 3 },
			{ 1, 2 }
		};
	
		/// *** ///
		
		// X
		// O
		// X

		// X-X
		public static readonly int[,] MatchPatternVXOXRequiredTiles =
		{
			{ 0, 0 },
			{ 0, 2 }
		};

		//    Left - Right
		//  -10      0
		// 2  X      X
		// 1 M-      -M
		// 0  X      X
		public static readonly int[,] MatchPatternVXOXMatchTiles =
		{
			{ -1, 1 },
			{ 1, 1 }
		};
	
		/// *** ///
		
		// O
		// X
		// X

		// -XX
		public static readonly int[,] MatchPatternVOXXRequiredTiles =
		{
			{ 0, 1 },
			{ 0, 2 }
		};

		//     Left - Below - Right
		//   -101     0       01
		//  2  X      X       X
		//  1  X      X       X
		//  0 M-      O       -M
		// -1         M
		public static readonly int[,] MatchPatternVOXXMatchTiles =
		{
			{ -1, 0 },
			{ 0, -1 },
			{ 1, 0 }
		};
	};
}
