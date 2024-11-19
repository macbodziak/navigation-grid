using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Navigation
{
    public class Actor : MonoBehaviour
    {
        #region Fields
        [SerializeField] NavGrid _grid;
        [SerializeField] int _currentNodeIndex;
        int _previousNodeIndex = -1;
        [SerializeField] float _speed;
        [SerializeField] float _speedModifier;
        [SerializeField] float _rotationSpeed;
        bool _cancelFlag = false;
        [SerializeField] ActorState _state = ActorState.Uninitilized;
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
        public ActorState State { get => _state; }
        public int NodeIndex { get => _currentNodeIndex; }

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

        public void Initilize(NavGrid navGrid, int nodeIndex)
        {
            _grid = navGrid;
            _previousNodeIndex = -1;
            _currentNodeIndex = nodeIndex;
            transform.position = _grid.WorldPositionAt(_currentNodeIndex);
            _state = ActorState.Idle;
        }

        public void Deinitialize()
        {
            _grid = null;
            _currentNodeIndex = -1;
            _previousNodeIndex = -1;
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
                _previousNodeIndex = _currentNodeIndex;
                _currentNodeIndex = _path[_pathIndex].gridIndex;
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

                _pathIndex--;

                if (_cancelFlag || _pathIndex < 0)
                {
                    OnMovementFinished();
                }
            }
        }


        public void Teleport(int nodeIndex)
        {
            if (_state != ActorState.Idle)
            {
                return;
            }

            if (nodeIndex == _currentNodeIndex)
            {
                return;
            }

            if (_grid.CheckIfInBound(nodeIndex) == false)
            {
                return;
            }

            if (_grid.IsWalkable(nodeIndex) == false)
            {
                return;
            }

            _previousNodeIndex = _currentNodeIndex;
            _currentNodeIndex = nodeIndex;

            OnNodeExiting();
            transform.position = _grid.WorldPositionAt(nodeIndex);

            OnNodeEntered();

        }

        public void Teleport(Vector2Int coordinates)
        {
            Teleport(_grid.IndexAt(coordinates));
        }


        public void Teleport(int x, int z)
        {
            Teleport(_grid.IndexAt(x, z));
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

        public void FaceTowardsInstantly(Vector3 worldPosition)
        {
            if (_state != ActorState.Moving || _state != ActorState.Paused)
            {
                transform.rotation = Quaternion.LookRotation(worldPosition - transform.position);
            }
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
            _previousNodeIndex = _currentNodeIndex;

            _cancelFlag = false;

            MovementStartedEvent?.Invoke(this, new ActorStartedMovementEventArgs(_currentNodeIndex));
        }

        private void OnMovementFinished()
        {
            _cancelFlag = false;
            _state = ActorState.Idle;
            _pathIndex = -1;

            MovementFinishedEvent?.Invoke(this, new ActorFinishedMovementEventArgs(_currentNodeIndex));
        }

        private void OnNodeExiting()
        {
            _grid.OnActorExitsNode(this, _previousNodeIndex, _currentNodeIndex);

            NodeExitedEvent?.Invoke(this, new ActorExitedNodeEventArgs(_previousNodeIndex, _currentNodeIndex));
        }

        private void OnNodeEntered()
        {
            NodeEnteredEvent?.Invoke(this, new ActorEnteredNodeEventArgs(_previousNodeIndex, _currentNodeIndex));
        }
        #endregion

    }

}