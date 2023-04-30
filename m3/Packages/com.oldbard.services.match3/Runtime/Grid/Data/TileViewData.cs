using System;
using UnityEngine;

namespace OldBard.Services.Match3.Grid.Data
{
    /// <summary>
    /// Struct to hold view information for the Tiles creation
    /// </summary>
    [Serializable]
    public struct TileViewData
    {
        public Sprite Body;
        public Sprite Eye;
        public Sprite Mouth;
        public Sprite Shadow;
        public Sprite Selected;
        public RuntimeAnimatorController Animation;
    }
}
