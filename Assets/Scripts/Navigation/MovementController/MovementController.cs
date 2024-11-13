using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Navigation
{
    public class MovementController : MonoBehaviour
    {
        #region Fields

        [SerializeField] Vector2Int _gridPosition;
        [SerializeField] NavGrid _grid;
        [SerializeField] float _speed;
        [SerializeField] float _speedModifier;
        [SerializeField] float _rotationSpeed;
        bool _cancelFlag = false;
        [SerializeField] MovementControllerState _state;
        Path _path;

        public float Speed { get => _speed; set => _speed = value; }
        public float SpeedModifier
        {
            get => _speedModifier;
            set
            {
                if (value > 0f)
                {
                    _speedModifier = value;
                }
                else
                {
                    _speedModifier = 0f;
                }
            }
        }
        internal MovementControllerState State { get => _state; }
        public Vector2Int GridPosition { get => _gridPosition; }
        public float RotationSpeed
        {
            get => _rotationSpeed;
            set
            {
                if (value > 0f)
                {
                    _rotationSpeed = value;
                }
                else
                {
                    _rotationSpeed = 0f;
                }
            }
        }


        #endregion

        #region Methods

        public void Initilize()
        {
            throw new NotImplementedException();
        }

        public void MoveAlongPath(Path path)
        {
            if (path == null)
            {
                return;
            }

            if (_state != MovementControllerState.Idle)
            {
                return;
            }

            _path = path;

            StartCoroutine(MoveAlongPathRoutine());
        }

        private IEnumerator MoveAlongPathRoutine()
        {
            OnMovmentStarted();
            _state = MovementControllerState.Moving;
            int pathIndex = _path.Count - 1;
            _cancelFlag = false;
            Vector3 targetPosition;
            Quaternion targetRotation;

            while (pathIndex >= 0)
            {
                OnNodeExited();

                targetPosition = _path[pathIndex].worldPosition;
                targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

                while (transform.position != targetPosition)
                {
                    if (_state == MovementControllerState.Moving)
                    {
                        float delta = _speed * _speedModifier * Time.deltaTime;
                        float rotationDelta = _rotationSpeed * _speedModifier * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, delta);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationDelta);
                    }
                    yield return null;
                }

                OnNodeEntered();
                _gridPosition = _path[pathIndex].gridPosition;
                //update nav grid 
                pathIndex--;

                if (_cancelFlag || pathIndex < 0)
                {
                    OnMovementFinished();
                    _cancelFlag = false;
                    _state = MovementControllerState.Idle;
                }
            }
        }

        public void Pause()
        {
            if (_state == MovementControllerState.Moving)
            {
                _state = MovementControllerState.Paused;
            }
        }

        public void Continue()
        {
            if (_state == MovementControllerState.Paused)
            {
                _state = MovementControllerState.Moving;
            }
        }

        public void Cancel()
        {
            _cancelFlag = true;
        }

        private void OnMovmentStarted()
        {
            Debug.Log(gameObject + "OnMovmentStarted() ");
        }

        private void OnMovementFinished()
        {
            Debug.Log(gameObject + "OnMovementFinished() ");
        }

        private void OnNodeExited()
        {
            Debug.Log(gameObject + "OnNodeExit() ");
        }

        private void OnNodeEntered()
        {
            Debug.Log(gameObject + "OnNodeEnter() ");
        }

        #endregion

    }

}