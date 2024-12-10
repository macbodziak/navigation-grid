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
        public event Action<ActorStartedMovementEventArgs> MovementStartedEvent;
        public event Action<ActorFinishedMovementEventArgs> MovementFinishedEvent;
        public event Action<ActorEnteredNodeEventArgs> NodeEnteredEvent;
        public event Action<ActorExitedNodeEventArgs> NodeExitedEvent;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the actor by setting its grid reference and starting position.
        /// </summary>
        public void Initilize(NavGrid navGrid, int nodeIndex)
        {
            _grid = navGrid;
            _previousNodeIndex = -1;
            _currentNodeIndex = nodeIndex;
            transform.position = _grid.WorldPositionAt(_currentNodeIndex);
            _state = ActorState.Idle;
        }

        /// <summary>
        /// Deinitializes the actor by clearing grid references and resetting its state.
        /// </summary>
        public void Deinitialize()
        {
            _grid = null;
            _currentNodeIndex = -1;
            _previousNodeIndex = -1;
            _state = ActorState.Uninitilized;
        }

        /// <summary>
        /// Initializes non-serialized fields during the Awake phase.
        /// </summary>
        private void Awake()
        {
            //unserialized fields need to be initialized
            _previousNodeIndex = -1;
            _state = ActorState.Idle;

            _tokenSource = new();
        }

        /// <summary>
        /// Cleans up resources, such as token sources, when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            _grid?.RemoveActor(_currentNodeIndex);
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        /// <summary>
        /// Moves the actor along a given path asynchronously.
        /// returns a Task to evaluate the state of execution.
        /// Throws an OperationCanceledException exception if the Actor is deleted before task completes.
        /// </summary>
        public async Task MoveAlongPathAsync(Path path, CancellationToken cancellationToken)
        {
            CancellationToken thisCancelationToken = _tokenSource.Token;

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

                    //this is the internal cancellation token, that should throw an exception when this componenet gets destroyed before finishing
                    if (thisCancelationToken.IsCancellationRequested)
                    {
                        thisCancelationToken.ThrowIfCancellationRequested();
                        return;
                    }
                }

                OnNodeEntered();

                _pathIndex++;

                if (_cancelFlag || _pathIndex == _path.Count || cancellationToken.IsCancellationRequested)
                {
                    _cancelFlag = false;
                    _state = ActorState.Idle;
                    break;
                }
            }
            MovementFinishedEvent?.Invoke(new ActorFinishedMovementEventArgs(this, _grid, _currentNodeIndex));
        }

        /// <summary>
        /// Starts the actor's movement along a path using a coroutine.
        /// </summary>
        public void MoveAlongPath(Path path)
        {
            StartCoroutine(MoveAlongPathCoroutine(path));
        }

        /// <summary>
        /// Coroutine that handles the actor's movement along a path.
        /// </summary>
        public IEnumerator MoveAlongPathCoroutine(Path path)
        {
            if (path == null)
            {
                yield break;
            }

            if (_state != ActorState.Idle)
            {
                yield break;
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
                    yield return null;
                }

                OnNodeEntered();

                _pathIndex++;

                if (_cancelFlag || _pathIndex == _path.Count)
                {
                    _cancelFlag = false;
                    _state = ActorState.Idle;
                    break;
                }
            }
            MovementFinishedEvent?.Invoke(new ActorFinishedMovementEventArgs(this, _grid, _currentNodeIndex));
        }

        /// <summary>
        /// Updates the target rotation and resets rotation progress if the direction changes.
        /// </summary>
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

        /// <summary>
        /// Teleports the actor to a specific grid node.
        /// </summary>
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

        /// <summary>
        /// Teleports the actor to specific grid coordinates.
        /// </summary>
        public void Teleport(Vector2Int coordinates)
        {
            Teleport(_grid.IndexAt(coordinates));
        }

        /// <summary>
        /// Teleports the actor to specific grid coordinates.
        /// </summary>
        public void Teleport(int x, int z)
        {
            Teleport(_grid.IndexAt(x, z));
        }

        /// <summary>
        /// Makes the actor face towards a specific world position asynchronously.
        /// </summary>
        public async Task FaceTowardsAsync(Vector3 worldPosition)
        {
            if (_state != ActorState.Idle)
            {
                return;
            }

            _state = ActorState.Moving;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
            float progress = 0f;

            while (progress <= 1)
            {
                progress += _rotationSpeed * _speedModifier * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                Debug.Log($"progress {progress}, transform {transform.rotation}");
                await Awaitable.NextFrameAsync();
            }
            _state = ActorState.Idle;
        }

        /// <summary>
        /// Makes the actor face towards a specific world position using a coroutine.
        /// </summary>
        public void FaceTowards(Vector3 worldPosition)
        {
            StartCoroutine(FaceTowardsCoroutine(worldPosition));
        }

        /// <summary>
        /// Coroutine for rotating the actor to face towards a specific world position.
        /// </summary>
        private IEnumerator FaceTowardsCoroutine(Vector3 worldPosition)
        {
            if (_state != ActorState.Idle)
            {
                yield break;
            }
            _state = ActorState.Moving;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
            float progress = 0f;

            while (progress < 1)
            {
                progress += _rotationSpeed * _speedModifier * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                yield return null;
            }
            _state = ActorState.Idle;
            yield return null;
        }



        /// <summary>
        /// Instantly rotates the actor to face a specific position.
        /// </summary>
        public void FaceTowardsInstantly(Vector3 worldPosition)
        {
            if (_state != ActorState.Moving || _state != ActorState.Paused)
            {
                transform.rotation = Quaternion.LookRotation(worldPosition - transform.position);
            }
        }

        /// <summary>
        /// Pauses the actor's movement.
        /// </summary>
        public void Pause()
        {
            if (_state == ActorState.Moving)
            {
                _state = ActorState.Paused;
            }
        }

        /// <summary>
        /// Resumes the actor's movement if paused.
        /// </summary>
        public void Continue()
        {
            if (_state == ActorState.Paused)
            {
                _state = ActorState.Moving;
            }
        }

        /// <summary>
        /// Cancels the current movement by setting the cancellation flag.
        /// If Actor is in between nodes, it will finish moving to the next Node before stopping.
        /// </summary>
        public void Cancel()
        {
            _cancelFlag = true;
        }

        /// <summary>
        /// Handles logic when movement starts, such as setting the state, path index, and invoking the movement start event.
        /// </summary>
        private void OnMovementStarted()
        {
            _state = ActorState.Moving;

            // we start at 1, as 0 is the point we already are at 
            _pathIndex = 1;
            _previousNodeIndex = _currentNodeIndex;

            _cancelFlag = false;

            MovementStartedEvent?.Invoke(new ActorStartedMovementEventArgs(this, _grid, _currentNodeIndex));
        }

        /// <summary>
        /// Invokes events and handles logic when the actor exits a node.
        /// </summary>
        private void OnNodeExiting()
        {
            _grid.OnActorExitsNode(this, _previousNodeIndex, _currentNodeIndex);

            NodeExitedEvent?.Invoke(new ActorExitedNodeEventArgs(this, _grid, _previousNodeIndex, _currentNodeIndex));
        }

        /// <summary>
        /// Invokes events and handles logic when the actor enters a new node.
        /// </summary>
        private void OnNodeEntered()
        {
            NodeEnteredEvent?.Invoke(new ActorEnteredNodeEventArgs(this, _grid, _previousNodeIndex, _currentNodeIndex));
        }
        #endregion

    }

}