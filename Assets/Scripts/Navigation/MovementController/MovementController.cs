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
        int _pathIndex;
        Vector3 _targetPosition;
        Quaternion _targetRotation;
        #endregion

        #region Properties
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

        #region Events
        public event EventHandler MovementStartedEvent;
        public event EventHandler MovementFinishedEvent;
        public event EventHandler<int> NodeEnteredEvent;
        public event EventHandler<int> NodeExitedEvent;

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

            StartCoroutine(MoveAlongPathCoroutine());
        }

        private IEnumerator MoveAlongPathCoroutine()
        {
            OnMovmentStarted();

            while (_pathIndex >= 0)
            {
                OnNodeLeaving();

                _targetPosition = _path[_pathIndex].worldPosition;
                _targetRotation = Quaternion.LookRotation(_targetPosition - transform.position);

                while (transform.position != _targetPosition)
                {
                    if (_state == MovementControllerState.Moving)
                    {
                        float delta = _speed * _speedModifier * Time.deltaTime;
                        float rotationDelta = _rotationSpeed * _speedModifier * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, delta);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, rotationDelta);
                    }
                    yield return null;
                }

                OnNodeEntered();
            }
        }

        public void FaceTowards(Vector3 worldPosition)
        {
            if (_state != MovementControllerState.Idle)
            {
                return;
            }
            StartCoroutine(FaceTowardsCoroutine(worldPosition));
        }

        private IEnumerator FaceTowardsCoroutine(Vector3 worldPosition)
        {
            _state = MovementControllerState.Moving;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
            float progress = _speedModifier * Time.deltaTime * 2f;

            while (progress < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                progress += _speedModifier * Time.deltaTime * 2f;
                yield return null;
            }
            _state = MovementControllerState.Idle;
            yield return null;
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
            _state = MovementControllerState.Moving;
            _pathIndex = _path.Count - 1;
            _cancelFlag = false;

            MovementStartedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnMovementFinished()
        {
            _cancelFlag = false;
            _state = MovementControllerState.Idle;
            MovementFinishedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnNodeLeaving()
        {
            //TO DO - inform navGrid about it
            NodeExitedEvent?.Invoke(this, _path[_pathIndex].gridIndex);
        }

        private void OnNodeEntered()
        {
            _gridPosition = _path[_pathIndex].gridPosition;
            //TO-DO update nav grid 

            NodeEnteredEvent?.Invoke(this, _path[_pathIndex].gridIndex);

            _pathIndex--;

            if (_cancelFlag || _pathIndex < 0)
            {
                OnMovementFinished();
            }
        }
        #endregion

    }

}