using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OldBard.Match3.Gameplay.Controllers
{
    public class InputManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        bool _lockInput;
        bool _isDragging;
        Vector3 _initialDragPos = Vector3.negativeInfinity;
        DateTime _lastInteractionTime;

        public Action<Vector3, Vector3> Dragged;

        #region Interactions

        /// <summary>
        /// Returns true if the player is allowed to interact with the board
        /// </summary>
        public bool CanDrag
        {
            get => !_lockInput;
        }

        /// <summary>
        /// Returns the amount of seconds elapased since the last interaction
        /// </summary>
        public double TimeSinceLastInteraction
        {
            get => (DateTime.UtcNow - _lastInteractionTime).TotalSeconds;
        }

        /// <summary>
        /// Allow the player to interact with the board
        /// </summary>
        public void EnableInput()
        {
            _lockInput = false;
            _initialDragPos = Vector3.negativeInfinity;
            _lastInteractionTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Blocks the player from interacting with the board
        /// </summary>
        public void DisableInput()
        {
            _lockInput = true;
        }

        /// <summary>
        /// Resets the timer for the last time there was an interaction
        /// </summary>
        public void ResetLastInteraction()
        {
            _lastInteractionTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Unity's input callback with the data about dragging
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag) return;

            _lastInteractionTime = DateTime.UtcNow;

            _isDragging = true;

            _initialDragPos = Input.mousePosition;
        }

        /// <summary>
        /// Unity's input callback with the data about dragging
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _initialDragPos = Vector3.negativeInfinity;
        }

        /// <summary>
        /// Unity's input callback with the data about dragging
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            var curPos = Input.mousePosition;

            if (!_isDragging || curPos == _initialDragPos || !CanDrag) return;

            Dragged?.Invoke(curPos, _initialDragPos);
        }

        #endregion
    }
}