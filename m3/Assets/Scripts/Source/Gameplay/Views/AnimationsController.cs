using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OldBard.Services.Match3.Grid;
using TMPro;
using UnityEngine;

namespace OldBard.Match3.Gameplay.Views.Animations
{
    public class AnimationsController
    {
        /// Declarations
        
        readonly AnimationCurve _animationCurve;

        public AnimationsController(AnimationCurve curve)
        {
            _animationCurve = curve;
        }

        /// Animations
        /// <summary>
        /// Animation of tiles position in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        /// <param name="totalTime">Duration of the animation</param>
        public async Task PlayTilesPositionAnim(IReadOnlyList<TileInstance> tiles, float totalTime)
        {
            await PlayTilesAnim(tiles, AnimateTilePosition, totalTime);
        }

        /// <summary>
        /// Animation of tiles scale in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        /// <param name="totalTime">Duration of the animation</param>
        public async Task PlayTilesScaleAnim(List<TileInstance> tiles, float totalTime)
        {
            await PlayTilesAnim(tiles, AnimateTileScale, totalTime);
        }

        /// <summary>
        /// Animation of tiles background alpha in the board
        /// </summary>
        /// <param name="tiles">List of Tiles to animate</param>
        /// <param name="totalTime">Duration of the animation</param>
        /// <param name="showing">Whether the tile is being shown or hidden</param>
        public async Task PlayTilesBackgroundAlphaAnim(List<TileInstance> tiles,
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
        /// <param name="animationCall">Action to call at every frame</param>
        /// <param name="totalTime">Duration of the animation</param>
        async Task PlayTilesAnim(IReadOnlyList<TileInstance> tiles,
            Action<TileInstance, float, float> animationCall,
            float totalTime)
        {
            await PlayAnim((elapsedTime) =>
                {
                    foreach(TileInstance tile in tiles)
                    {
                        animationCall(tile, elapsedTime, totalTime);
                    }
                },
            totalTime);
        }

        /// <summary>
        /// Animation of tiles in the board
        /// </summary>
        /// <param name="text">Text to change the color</param>
        /// <param name="initColor">Initial Color</param>
        /// <param name="endColor">End Color</param>
        /// <param name="totalTime">Duration of the animation</param>
        public async Task PlayTextColorAnim(TextMeshProUGUI text, Color initColor, Color endColor, float totalTime)
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
        /// <param name="lerpCallback">Action to call at every frame</param>
        /// <param name="totalTime">Duration of the animation</param>
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
        /// <param name="elapsedTime">Current elapsed time</param>
        /// <param name="totalTime">Duration of the animation</param>
        void AnimateTilePosition(TileInstance tile,
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
        /// <param name="elapsedTime">Current elapsed time</param>
        /// <param name="totalTime">Duration of the animation</param>
        void AnimateTileScale(TileInstance tile,
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
        /// <param name="elapsedTime">Current elapsed time</param>
        /// <param name="totalTime">Duration of the animation</param>
        void AnimateTileBackgroundAlphaShow(TileInstance tile,
            float elapsedTime, float totalTime)
        {
            AnimateTileBackgroundAlpha(tile, elapsedTime, totalTime, 0f, 1f);
        }

        /// <summary>
        /// Animate background alpha of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        /// <param name="elapsedTime">Current elapsed time</param>
        /// <param name="totalTime">Duration of the animation</param>
        void AnimateTileBackgroundAlphaHide(TileInstance tile,
            float elapsedTime, float totalTime)
        {
            AnimateTileBackgroundAlpha(tile, elapsedTime, totalTime, 1f, 0f);
        }

        /// <summary>
        /// Animate background alpha of the tile in the board
        /// </summary>
        /// <param name="tile">Tile to animate</param>
        /// <param name="elapsedTime">Current elapsed time</param>
        /// <param name="totalTime">Duration of the animation</param>
        /// <param name="initialAlpha">Initial Alpha</param>
        /// <param name="endAlpha">End Alpha</param>
        void AnimateTileBackgroundAlpha(TileInstance tile,
            float elapsedTime, float totalTime, float initialAlpha, float endAlpha)
        {
            tile.TileView.SetSelectedBackgroundAlpha(
                Mathf.Lerp(initialAlpha, endAlpha, _animationCurve.Evaluate(elapsedTime / totalTime)));
        }
    }
}
