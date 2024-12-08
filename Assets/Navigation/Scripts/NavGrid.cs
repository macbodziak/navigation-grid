using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Utils;

namespace Navigation
{
    public abstract class NavGrid : MonoBehaviour
    {
        #region Fields
        [SerializeField] float tileSize;
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] protected Node[] nodes;
        [SerializeField] protected Vector3[] nodeWorldPositions;
        [SerializeField] protected SerializableDictionary<int, Actor> actors;

#if UNITY_EDITOR
        [SerializeField] bool showTileOutlineFlag = true;
        [SerializeField] bool showTileCenterFlag = true;
        [SerializeField] bool showTileInfoTextFlag = false;
        [SerializeField] bool ShowNodeGridCoordinatesTextFlag = true;
        [SerializeField] bool ShowNodeWalkableTextFlag = false;
        [SerializeField] bool ShowNodeMovementCostTextFlag = false;
        [SerializeField] bool ShowOccupyingActorTextFlag = false;
        [SerializeField] int TileInfoTextFontSize = 10;
        [SerializeField] Color TileInfoTextColor = Color.white;
#endif
        #endregion

        #region Properties
        public int Width { get => width; protected set => width = value; }
        public int Height { get => height; protected set => height = value; }
        public int Count { get => Height * Width; }
        public Vector3 Position { get => transform.position; }
        public float TileSize { get => tileSize; protected set => tileSize = value; }
        public abstract Bounds WorldBounds { get; }

#if UNITY_EDITOR
        protected bool DebugDrawTileOutline { get => showTileOutlineFlag; }
        protected bool DebugDrawTileCenter { get => showTileCenterFlag; }
#endif
        #endregion

        /// <summary>
        /// This method initializes the grid. It creates the node array and configures each node with its index, position, and walkability status.
        /// It also configures the map's collision box. 
        /// </summary>
        public void CreateMap(int width, int height, float tileSize, LayerMask notWalkableLayers, int collisionLayer, float colliderSize, float rayLength)
        {
            this.Width = width;
            this.Height = height;
            this.TileSize = tileSize;

            nodes = new Node[width * height];
            nodeWorldPositions = new Vector3[width * height];
            actors = new();

            int gridIndex;
            bool walkable;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    gridIndex = IndexAt(w, h);
                    nodeWorldPositions[gridIndex] = GridCoordinatesToWorldPosition(w, h);
                    walkable = TestForWalkability(nodeWorldPositions[gridIndex], notWalkableLayers, colliderSize, rayLength);
                    nodes[gridIndex].Setup(gridIndex, w, h, walkable);
                }
            }

            SetupCollider(collisionLayer);
        }

        /// <summary>
        /// Tests if a node is walkable based on its position and collision detection.
        /// </summary>
        protected abstract bool TestForWalkability(Vector3 nodeWorldPosition, LayerMask notWalkableLayers, float colliderSize, float rayLength);


        /// <summary>
        /// Sets up a collider to represent the grid's boundaries.
        /// </summary>
        protected abstract void SetupCollider(int collisionLayer);


        /// /// <summary>
        /// Scans the scene for actors and installs them on the grid if they are above walkable nodes.
        /// </summary>
        public void ScanForActors(float rayLength = 100f)
        {
            Actor[] actorsInScene = FindObjectsByType<Actor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Actor actor in actorsInScene)
            {
                Ray ray = new Ray(actor.transform.position + Vector3.up, Vector3.down * rayLength);
                RaycastHit hitInfo;
                LayerMask layerMask = 1 << gameObject.layer;

                if (Physics.Raycast(ray, out hitInfo, rayLength, layerMask))
                {
                    int nodeIndex = IndexAt(hitInfo.point);
                    if (nodeIndex != -1)
                    {
                        if (IsWalkable(nodeIndex))
                        {
                            InstallActor(actor, nodeIndex);
                        }
                    }
                }
            }
        }

        #region Index Getters
        /// <summary>
        /// Returns the index of a node at the specified grid coordinates.
        /// </summary>
        public int IndexAt(int x, int z)
        {
            if (x >= Width || x < 0 || z >= Height || z < 0)
            {
                return -1;
            }
            return x + z * Width;
        }

        /// <summary>
        /// Returns the index of a node at the specified grid position.
        /// </summary>
        public int IndexAt(Vector2Int p)
        {
            return IndexAt(p.x, p.y);
        }

        /// <summary>
        /// Returns the index of a node based on a world position.
        /// </summary>
        public int IndexAt(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldPositionToGridCoordinates(worldPosition);
            return IndexAt(gridPos.x, gridPos.y);
        }
        #endregion


        #region Node Getters
        /// <summary>
        /// Retrieves the node at the specified index or returns a null node if the index is invalid.
        /// </summary>
        public Node NodeAt(int index)
        {
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[index];
        }

        /// <summary>
        /// Retrieves the node at the specified grid coordinates or returns a null node if the coordinates are invalid.
        /// </summary>
        public Node NodeAt(int x, int z)
        {
            int index = IndexAt(x, z);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[IndexAt(x, z)];
        }

        /// <summary>
        /// Retrieves the node at the specified grid position.
        /// </summary>
        public Node NodeAt(Vector2Int gridPosition)
        {
            int index = IndexAt(gridPosition);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[IndexAt(gridPosition)];
        }

        /// <summary>
        /// Retrieves the node at the specified world position.
        /// </summary>
        public Node NodeAt(Vector3 worldPosition)
        {
            int index = IndexAt(worldPosition);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[IndexAt(worldPosition)];
        }

        #endregion


        #region Grid Position Getters
        /// <summary>
        /// Converts a node index to grid coordinates.
        /// </summary>
        public Vector2Int GridCoordinatesAt(int index)
        {
            int x = index % width;
            int z = index / width;
            return new Vector2Int(x, z);
        }
        #endregion

        /// <summary>
        /// Converts a world position to grid coordinates.
        /// </summary>
        public Vector2Int GridCoordinatesAt(Vector3 worldPosition)
        {
            return WorldPositionToGridCoordinates(worldPosition);
        }


        #region World Position Getters and Conversion
        /// <summary>
        /// Returns the world position of a node at the specified grid coordinates.
        /// </summary>
        public Vector3 WorldPositionAt(int x, int z)
        {
            return nodeWorldPositions[IndexAt(x, z)];
        }

        /// <summary>
        /// Returns the world position of a node at the specified grid position.
        /// </summary>
        public Vector3 WorldPositionAt(Vector2Int p)
        {
            return nodeWorldPositions[IndexAt(p)];
        }

        /// <summary>
        /// Returns the world position of a node at the specified index.
        /// </summary>
        public Vector3 WorldPositionAt(int index)
        {
            return nodeWorldPositions[index];
        }

        /// <summary>
        /// Converts a world position to grid coordinates.
        /// </summary>
        protected abstract Vector2Int WorldPositionToGridCoordinates(Vector3 worldPosition);

        /// <summary>
        /// Converts grid coordinates to a world position.
        /// </summary>
        protected abstract Vector3 GridCoordinatesToWorldPosition(int x, int z);

        #endregion


        #region Walkability Checkers
        /// <summary>
        /// Checks if the node at the specified index is walkable and not occupied.
        /// </summary>
        public bool IsWalkable(int index)
        {
            return nodes[index].walkable && actors.ContainsKey(index) == false;
        }

        /// <summary>
        /// Checks if the node at the specified grid coordinates is walkable and not occupied.
        /// </summary>
        public bool IsWalkable(int x, int z)
        {
            return IsWalkable(IndexAt(x, z));
        }

        /// <summary>
        /// Checks if the node at the specified grid position is walkable and not occupied.
        /// </summary>
        public bool IsWalkable(Vector2Int gridPosition)
        {
            return IsWalkable(IndexAt(gridPosition));
        }

        #endregion


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (showTileInfoTextFlag)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.fontSize = TileInfoTextFontSize;

                Color cachedColor = GUI.color;
                GUI.color = GUI.color = TileInfoTextColor;

                Plane[] planes = GetEditorCameraFrustrumPlanes();

                foreach (Node n in nodes)
                {
                    //check if the label will be visible on screen, otherwise do not print
                    //this is to prevent lag in case of many nodes (more than 10,000)
                    if (GeometryUtility.TestPlanesAABB(planes, new Bounds(nodeWorldPositions[n.id], Vector3.one)))
                    {
                        DrawTileInfoText(n, style);
                    }
                }

                GUI.color = cachedColor;
            }

            Gizmos.color = Color.white;
            if (nodes == null)
            {
                Handles.Label(transform.position, "No Map Generated");
                return;
            }

            foreach (Node n in nodes)
            {
                if (n.walkable == true)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                DrawNodeCenterOutineGizmos(n);
            }
#endif
        }

        private Plane[] GetEditorCameraFrustrumPlanes()
        {
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(sceneCamera);
            float customFarDistance = 25f * tileSize;

            Vector3 cameraPosition = sceneCamera.transform.position;
            Vector3 cameraForward = sceneCamera.transform.forward;

            // Create a new far clipping plane closer to the camera
            Plane customFarPlane = new Plane(-cameraForward, cameraPosition + cameraForward * customFarDistance);

            // Replace the far plane (assuming index 5 is the far plane, based on order)
            planes[5] = customFarPlane;
            return planes;
        }

#if UNITY_EDITOR
        protected abstract void DrawNodeCenterOutineGizmos(Node node);
#endif

#if UNITY_EDITOR
        protected void DrawTileInfoText(Node node, GUIStyle style)
        {
            bool addSeperator = false;
            string infoText = " ";

            if (ShowNodeGridCoordinatesTextFlag)
            {
                infoText += $"({node.gridCoordinates.x},{node.gridCoordinates.y}) ";
                addSeperator = true;
            }

            if (ShowNodeWalkableTextFlag)
            {
                if (addSeperator == true)
                {
                    infoText += "| ";
                }
                infoText += $"{node.walkable} ";
                addSeperator = true;
            }

            if (ShowNodeMovementCostTextFlag)
            {
                if (addSeperator == true)
                {
                    infoText += "| ";
                }
                infoText += $"{node.movementCostModifier} ";
                addSeperator = true;
            }

            if (ShowOccupyingActorTextFlag)
            {
                if (actors.ContainsKey(node.id))
                {
                    if (addSeperator == true)
                    {
                        infoText += "| ";
                    }
                    if (actors[node.id] != null)
                    {
                        infoText += actors[node.id].gameObject.name;
                    }
                    else
                    {
                        infoText += "NULL";
                    }
                }
            }


            Handles.Label(nodeWorldPositions[node.id], infoText, style);
        }
#endif


        #region Bound Checking
        /// <summary>
        /// Checks whether the given grid coordinates are within the grid bounds.
        /// </summary>
        public bool CheckIfInBound(int x, int z)
        {
            if (x < 0 || x >= Width || z < 0 || z >= Height)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the specified grid position is within the grid bounds.
        /// </summary>
        public bool CheckIfInBound(Vector2Int coordinates)
        {
            if (coordinates.x < 0 || coordinates.x >= Width || coordinates.y < 0 || coordinates.y >= Height)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether the given index is within the grid bounds.
        /// </summary>
        public bool CheckIfInBound(int index)
        {
            if (index < 0 || index >= width * height)
            {
                return false;
            }
            return true;
        }

        #endregion
        /// <summary>
        /// Sets the movement cost modifier of a node at the specified index.
        /// </summary>
        public void SetMovementCostModifierAt(int index, float value)
        {
            nodes[index].movementCostModifier = value;
        }

        /// <summary>
        /// Sets the movement cost modifier of a node at the specified grid coordinates.
        /// </summary>
        public void SetMovementCostModifierAt(int x, int z, float value)
        {
            nodes[IndexAt(x, z)].movementCostModifier = value;
        }

        /// <summary>
        /// Retrieves the movement cost modifier of a node at the specified index.
        /// </summary>
        public float MovementCostModifierAt(int index)
        {
            return nodes[index].movementCostModifier;
        }

        /// <summary>
        /// Retrieves the movement cost modifier of a node at the specified grid coordinates.
        /// </summary>
        public float MovementCostModifierAt(int x, int z)
        {
            return nodes[IndexAt(x, z)].movementCostModifier;
        }

        /// <summary>
        /// Returns a list of adjacent node indexes to the specified index.
        /// </summary>
        public abstract List<int> AdjacentNodeIndexes(int index);

        /// <summary>
        /// Returns a list of adjacent node indexes to the specified grid coordinates.
        /// </summary>
        public List<int> AdjacentNodeIndexes(int x, int z)
        {
            return AdjacentNodeIndexes(IndexAt(x, z));
        }

        /// <summary>
        /// Returns a list of adjacent node indexes to the specified grid position.
        /// </summary>
        public List<int> AdjacentNodeIndexes(Vector2Int coordinates)
        {
            return AdjacentNodeIndexes(IndexAt(coordinates));
        }

        /// <summary>
        /// Returns a list of adjacent node indexes to the specified world position.
        /// </summary>
        public List<int> AdjacentNodeIndexes(Vector3 worldPosition)
        {
            return AdjacentNodeIndexes(IndexAt(worldPosition));
        }

        /// <summary>
        /// Determines if two nodes at the specified indexes are adjacent.
        /// </summary>
        public abstract bool AreAdjacent(int firstIndex, int secondIndex);


        /// <summary>
        /// This method places the provided actor on the map at the given node index, registers it and sets it up
        /// </summary>
        public bool InstallActor(Actor actor, int index)
        {
            if (index < 0 || index >= width * height)
            {
                return false;
            }

            if (actors.ContainsKey(index))
            {
                return false;
            }

            actors[index] = actor;
            actor.Initilize(this, index);
#if UNITY_EDITOR
            EditorUtility.SetDirty(actor);
#endif
            return true;
        }


        /// <summary>
        /// This method places the provided actor on the map at the given x,y coordinates, registers it and sets it up
        /// </summary>
        public bool InstallActor(Actor actor, int x, int z)
        {
            return InstallActor(actor, IndexAt(x, z));
        }


        /// <summary>
        /// This method removes actor reference on the map at the given node index. 
        /// It does not alter the actor fields, thus should be used when actor gets Destoroyed
        /// </summary>
        public void RemoveActor(int index)
        {
            actors.Remove(index);
        }


        /// <summary>
        /// Uninstalls an actor from the grid, resetting its fields and clearing its reference.
        /// </summary>
        public void UninstallActor(int index)
        {
            if (actors.ContainsKey(index))
            {
                Actor actor = actors[index];
                actor.Deinitialize();
            }
            actors.Remove(index);
        }

        /// <summary>
        /// Uninstalls all actors from the grid and resets their references.
        /// </summary>
        public void UninstallAllActors()
        {
            foreach (var pair in actors)
            {
                if (pair.Value != null)
                {
                    pair.Value.Deinitialize();
                }
            }
            actors.Clear();
        }

        /// <summary>
        /// Retrieves the actor at the specified node index.
        /// </summary>
        public Actor ActorAt(int index)
        {
            if (actors.ContainsKey(index))
            {
                return actors[index];
            }

            return null;
        }

        /// <summary>
        /// Retrieves the actor at the specified grid coordinates.
        /// </summary>
        public Actor ActorAt(int x, int z)
        {
            return ActorAt(IndexAt(x, z));
        }

        /// <summary>
        /// Retrieves the actor at the specified grid coordinates.
        /// </summary>
        public Actor ActorAt(Vector2Int coordinates)
        {
            return ActorAt(IndexAt(coordinates));
        }

        /// <summary>
        /// Retrieves the actor at the specified world position.
        /// </summary>
        public Actor ActorAt(Vector3 worldPosition)
        {
            return ActorAt(IndexAt(worldPosition));
        }

        /// <summary>
        /// Returns a list of adjacent actors to the node at the specified index.
        /// </summary>
        public abstract List<Actor> AdjacentActors(int index);

        /// <summary>
        /// Returns a list of adjacent actors to the node at the specified grid coordinates.
        /// </summary>
        public List<Actor> AdjacentActors(int x, int z)
        {
            return AdjacentActors(IndexAt(x, z));
        }

        /// <summary>
        /// Returns a list of adjacent actors to the node at the specified grid position.
        /// </summary>
        public List<Actor> AdjacentActors(Vector2Int coordinates)
        {
            return AdjacentActors(IndexAt(coordinates));
        }

        /// <summary>
        /// Returns a list of adjacent actors to the node at the specified world position.
        /// </summary>
        public List<Actor> AdjacentActors(Vector3 worldPosition)
        {
            return AdjacentActors(IndexAt(worldPosition));
        }

        /// <summary>
        /// Handles logic when an actor exits a node and updates its reference to the new node.
        /// </summary>
        public void OnActorExitsNode(Actor actor, int FromIndex, int ToIndex)
        {
            if (FromIndex != -1)
            {
                actors.Remove(FromIndex);
            }

            if (actors.ContainsKey(ToIndex) == false)
            {
                actors[ToIndex] = actor;
            }
        }

        /// <summary>
        /// Returns a debug string with detailed information about the specified node.
        /// </summary>
        public string NodeDebugString(int index)
        {
            string debugText;
            debugText = "ID: " + nodes[index].id;
            debugText += $" ;   ({nodes[index].gridCoordinates.x},{nodes[index].gridCoordinates.y})";
            debugText += " ;   movement cost modifier: " + nodes[index].movementCostModifier;
            debugText += " ;   walkable: " + IsWalkable(index);
            if (actors.ContainsKey(index))
            {
                debugText += " ;   Actor: " + actors[index].gameObject.name;
            }
            else
            {
                debugText += " ;   Actor: NULL";
            }

            return debugText;
        }

        /// <summary>
        /// Retrieves all actors currently installed on the grid.
        /// </summary>
        public List<Actor> GetActors()
        {
            List<Actor> actorList = new();
            if (actors == null)
            {
                return actorList;
            }

            foreach (var kvp in actors)
            {
                actorList.Add(kvp.Value);
            }

            return actorList;
        }

    }
}