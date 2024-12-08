using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;

namespace Navigation
{
    public class Actor : MonoBehaviour
    {
        #region Fields
        [ReadOnly][SerializeField] NavGrid _grid;
        [ReadOnly][SerializeField] int _currentNodeIndex;
        int _previousNodeIndex = -1;
        [Tooltip("world units per second")]
        [SerializeField] float _speed = 1;
        [Tooltip("movement speed is multiplied by it. Default should be 1")]
        [SerializeField] float _speedModifier = 1;
        [Tooltip("Inverse of time needed to reach target rotation. 1 means it takes 1 second to reach new rotation.")]
        [SerializeField] float _rotationSpeed = 4;
        bool _cancelFlag = false;
        [ReadOnly][SerializeField] ActorState _state = ActorState.Uninitilized;
        Path _path;
        int _pathIndex;
        Vector3 _targetPosition;
        Quaternion _targetRotation;
        CancellationTokenSource _tokenSource;
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

        private void Awake()
        {
            //unserialized fields need to be initialized
            _previousNodeIndex = -1;
            _state = ActorState.Idle;

            _tokenSource = new();
        }


        private void OnDestroy()
        {
            _grid?.RemoveActor(_currentNodeIndex);
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        public async Task MoveAlongPathAsync(Path path)
        {
            CancellationToken cancelToken = _tokenSource.Token;

            if (path == null)
            {
                return;
            }

            if (_state != ActorState.Idle)
            {
                return;
            }

            _path = path;

            OnMovementStarted();

            Quaternion previousRotation = transform.rotation;
            float rotationProgress = 0f;

            while (_pathIndex < _path.Count)
            {
                _previousNodeIndex = _currentNodeIndex;
                _currentNodeIndex = _path[_pathIndex].nodeIndex;
                OnNodeExiting();

                _targetPosition = _path[_pathIndex].worldPosition;

                UpdateRotationTarget(ref rotationProgress, ref previousRotation);


                while (transform.position != _targetPosition)
                {
                    if (_state == ActorState.Moving)
                    {
                        float delta = _speed * _speedModifier * Time.deltaTime;
                        rotationProgress += _rotationSpeed * _speedModifier * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, delta);
                        transform.rotation = Quaternion.Slerp(previousRotation, _targetRotation, rotationProgress);
                    }
                    await Awaitable.NextFrameAsync();

                    if (cancelToken.IsCancellationRequested)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        return;
                    }
                }

                OnNodeEntered();

                _pathIndex++;

                if (_cancelFlag || _pathIndex == _path.Count)
                {
                    _cancelFlag = false;
                    _state = ActorState.Idle;
                }
            }
            MovementFinishedEvent?.Invoke(this, new ActorFinishedMovementEventArgs(_currentNodeIndex));
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
            OnMovementStarted();

            Quaternion previousRotation = transform.rotation;
            float rotationProgress = 0f;

            while (_pathIndex < _path.Count)
            {
                _previousNodeIndex = _currentNodeIndex;
                _currentNodeIndex = _path[_pathIndex].nodeIndex;
                OnNodeExiting();

                _targetPosition = _path[_pathIndex].worldPosition;

                UpdateRotationTarget(ref rotationProgress, ref previousRotation);


                while (transform.position != _targetPosition)
                {
                    if (_state == ActorState.Moving)
                    {
                        float delta = _speed * _speedModifier * Time.deltaTime;
                        rotationProgress += _rotationSpeed * _speedModifier * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, delta);
                        transform.rotation = Quaternion.Slerp(previousRotation, _targetRotation, rotationProgress);
                    }
                    yield return null;
                }

                OnNodeEntered();

                _pathIndex++;

                if (_cancelFlag || _pathIndex == _path.Count)
                {
                    _cancelFlag = false;
                    _state = ActorState.Idle;
                }
            }
            MovementFinishedEvent?.Invoke(this, new ActorFinishedMovementEventArgs(_currentNodeIndex));
        }

        private void UpdateRotationTarget(ref float rotationProgress, ref Quaternion previousRotation)
        {
            Quaternion nextTargetRotation = Quaternion.LookRotation(_targetPosition - transform.position);

            //use Dot Product to check if the target Rotation has changed, and if so reset rotation progress
            if (Mathf.Abs(Quaternion.Dot(_targetRotation, nextTargetRotation)) < 0.999f)
            {
                _targetRotation = nextTargetRotation;
                rotationProgress = 0f;
                previousRotation = transform.rotation;
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


        public async Task FaceTowardsAsync(Vector3 worldPosition)
        {
            if (_state != ActorState.Idle)
            {
                return;
            }

            _state = ActorState.Moving;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
            float progress = _rotationSpeed * _speedModifier * Time.deltaTime;

            while (progress < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                progress += _rotationSpeed * _speedModifier * Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }
            _state = ActorState.Idle;
            await Awaitable.NextFrameAsync();
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
            float progress = _rotationSpeed * _speedModifier * Time.deltaTime;

            while (progress < 1)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                progress += _rotationSpeed * _speedModifier * Time.deltaTime;
                yield return null;
            }
            _state = ActorState.Idle;
            yield return null;
        }



        ///--------

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

        private void OnMovementStarted()
        {
            _state = ActorState.Moving;

            // we start at 1, as 0 is the point we already are at 
            _pathIndex = 1;
            _previousNodeIndex = _currentNodeIndex;

            _cancelFlag = false;

            MovementStartedEvent?.Invoke(this, new ActorStartedMovementEventArgs(_currentNodeIndex));
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