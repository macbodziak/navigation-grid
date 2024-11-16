using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Navigation
{
    public class Actor : MonoBehaviour
    {
        #region Fields
        NavGrid _grid;
        int _gridIndex;
        [SerializeField] float _speed;
        [SerializeField] float _speedModifier;
        [SerializeField] float _rotationSpeed;
        bool _cancelFlag = false;
        [SerializeField] ActorState _state = ActorState.Uninitilized;
        Path _path;
        int _pathIndex;
        int _previousGridIndex;
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
        public ActorState State { get => _state; }
        public int GridIndex { get => _gridIndex; }

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
        public event EventHandler<ActorStartedMovementEventArgs> MovementStartedEvent;
        public event EventHandler<ActorFinishedMovementEventArgs> MovementFinishedEvent;
        public event EventHandler<ActorEnteredNodeEventArgs> NodeEnteredEvent;
        public event EventHandler<ActorExitedNodeEventArgs> NodeExitedEvent;

        #endregion

        #region Methods

        public void Initilize(NavGrid navGrid, int gridIndex)
        {
            _grid = navGrid;
            _gridIndex = gridIndex;
            transform.position = _grid.NodeWorldPositionAt(_gridIndex);
            _state = ActorState.Idle;
        }

        public void Deinitialize()
        {
            _grid = null;
            _gridIndex = -1;
            _previousGridIndex = -1;
            _state = ActorState.Uninitilized;
        }

        public void MoveAlongPath(Path path)
        {
            if (path == null)
            {
                return;
            }

            if (_state != ActorState.Idle)
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
                OnNodeExiting();

                _targetPosition = _path[_pathIndex].worldPosition;
                _targetRotation = Quaternion.LookRotation(_targetPosition - transform.position);

                while (transform.position != _targetPosition)
                {
                    if (_state == ActorState.Moving)
                    {
                        float delta = _speed * _speedModifier * Time.deltaTime;
                        float rotationDelta = _rotationSpeed * _speedModifier * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, delta);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, rotationDelta);
                    }
                    yield return null;
                }

                OnNodeEntered();

                _previousGridIndex = _path[_pathIndex].gridIndex;
                _pathIndex--;

                if (_cancelFlag || _pathIndex < 0)
                {
                    OnMovementFinished();
                }
            }
        }

        public void FaceTowards(Vector3 worldPosition)
        {
            if (_state != ActorState.Idle)
            {
                return;
            }
            StartCoroutine(FaceTowardsCoroutine(worldPosition));
        }

        private IEnumerator FaceTowardsCoroutine(Vector3 worldPosition)
        {
            _state = ActorState.Moving;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
            float progress = _speedModifier * Time.deltaTime * 2f;

            while (progress < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                progress += _speedModifier * Time.deltaTime * 2f;
                yield return null;
            }
            _state = ActorState.Idle;
            yield return null;
        }

        public void Pause()
        {
            if (_state == ActorState.Moving)
            {
                _state = ActorState.Paused;
            }
        }

        public void Continue()
        {
            if (_state == ActorState.Paused)
            {
                _state = ActorState.Moving;
            }
        }

        public void Cancel()
        {
            _cancelFlag = true;
        }

        private void OnMovmentStarted()
        {
            _state = ActorState.Moving;

            // -2 becuase the last entry is the starting point, which should be the point the actor is at right now
            _pathIndex = _path.Count - 2;
            _previousGridIndex = _gridIndex;

            _cancelFlag = false;

            MovementStartedEvent?.Invoke(this, new ActorStartedMovementEventArgs(_gridIndex));
        }

        private void OnMovementFinished()
        {
            _cancelFlag = false;
            _state = ActorState.Idle;

            MovementFinishedEvent?.Invoke(this, new ActorFinishedMovementEventArgs(_gridIndex));
        }

        private void OnNodeExiting()
        {
            _grid.OnActorExitsNode(this, _previousGridIndex, _path[_pathIndex].gridIndex);
            _gridIndex = _path[_pathIndex].gridIndex;

            NodeExitedEvent?.Invoke(this, new ActorExitedNodeEventArgs(_previousGridIndex, _path[_pathIndex].gridIndex));
        }

        private void OnNodeEntered()
        {
            NodeEnteredEvent?.Invoke(this, new ActorEnteredNodeEventArgs(_previousGridIndex, _path[_pathIndex].gridIndex));
        }
        #endregion

    }

}