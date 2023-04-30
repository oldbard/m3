using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OldBard.Match3.Gameplay.Views.Animations
{
    public class AnimationsController
    {
        #region Declarations

        AnimationCurve _animationCurve;

        public AnimationsController(AnimationCurve curve)
        {
            _animationCurve = curve;
        }

        #endregion

        #region Animations

        /// <summary>
        /// Animation of tiles position in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        public async Task PlayTilesPositionAnim(List<GridViewController.TileObjectView> tiles, float totalTime)
        {
            await PlayTilesAnim(tiles, AnimateTilePosition, totalTime);
        }

        /// <summary>
        /// Animation of tiles scale in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        public async Task PlayTilesScaleAnim(List<GridViewController.TileObjectView> tiles, float totalTime)
        {
            await PlayTilesAnim(tiles, AnimateTileScale, totalTime);
        }

        /// <summary>
        /// Animation of tiles background alpha in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        public async Task PlayTilesBackgroundAlphaAnim(List<GridViewController.TileObjectView> tiles,
            float totalTime, bool showing)
        {
            if(showing)
            {
                await PlayTilesAnim(tiles,
                    AnimateTileBackgroundAlphaShow,
                    totalTime);
            }
            else
            {
                await PlayTilesAnim(tiles,
                    AnimateTileBackgroundAlphaHide,
                    totalTime);
            }
        }

        /// <summary>
        /// Animation of tiles in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        async Task PlayTilesAnim(List<GridViewController.TileObjectView> tiles,
            Action<GridViewController.TileObjectView, float, float> animationCall,
            float totalTime)
        {
            await PlayAnim((elapsedTime) =>
            {
                for (var i = 0; i < tiles.Count; i++)
                {
                    animationCall(tiles[i], elapsedTime, totalTime);
                }
            },
            totalTime);
        }

        /// <summary>
        /// Animation of tiles in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        public async Task PlayTextColorAnim(Text text, Color initColor, Color endColor, float totalTime)
        {
            await PlayAnim((elapsedTime) =>
            {
                text.color = Color.LerpUnclamped(initColor, endColor, _animationCurve.Evaluate(elapsedTime / totalTime));
            },
            totalTime);
        }

        /// <summary>
        /// Does a call to an animation based on the elapsed time
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        async Task PlayAnim(Action<float> lerpCallback, float totalTime)
        {
            var elapsedTime = 0f;

            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;

                // Makes sure we don't pass the total time
                elapsedTime = Math.Min(elapsedTime, totalTime);

                lerpCallback(elapsedTime);

                // Wait
                await Task.Yield();
            }

            await Task.Yield();
        }

        /// <summary>
        /// Animate position of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        void AnimateTilePosition(GridViewController.TileObjectView tile,
            float elapsedTime, float totalTime)
        {
            tile.TileView.Position =
                Vector3.Lerp(
                    tile.TileView.Position,
                    tile.TileView.TargetPosition,
                    _animationCurve.Evaluate(elapsedTime / totalTime));
        }

        /// <summary>
        /// Animate scale of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        void AnimateTileScale(GridViewController.TileObjectView tile,
            float elapsedTime, float totalTime)
        {
            // Makes them bigger first to give a better feedback
            // The tile has [1, 1] by default
            var initialScale = new Vector3(2f, 2f, 2f);

            tile.TileView.LocalScale =
                Vector3.Lerp(
                    initialScale,
                    Vector3.zero,
                    _animationCurve.Evaluate(elapsedTime / totalTime));
        }

        /// <summary>
        /// Animate background alpha of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        void AnimateTileBackgroundAlphaShow(GridViewController.TileObjectView tile,
            float elapsedTime, float totalTime)
        {
            AnimateTileBackgroundAlpha(tile, elapsedTime, totalTime, 0f, 1f);
        }

        /// <summary>
        /// Animate background alpha of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        void AnimateTileBackgroundAlphaHide(GridViewController.TileObjectView tile,
            float elapsedTime, float totalTime)
        {
            AnimateTileBackgroundAlpha(tile, elapsedTime, totalTime, 1f, 0f);
        }

        /// <summary>
        /// Animate background alpha of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        void AnimateTileBackgroundAlpha(GridViewController.TileObjectView tile,
            float elapsedTime, float totalTime, float initialAlpha, float endAlpha)
        {
            tile.TileView.SetSelectedBackgroundAlpha(
                Mathf.Lerp(initialAlpha, endAlpha, _animationCurve.Evaluate(elapsedTime / totalTime)));
        }

        #endregion
    }
}
