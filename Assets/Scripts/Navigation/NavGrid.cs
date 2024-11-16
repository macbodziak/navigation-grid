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
        [SerializeField] SerializableDictionary<int, Actor> actors;

#if UNITY_EDITOR
        [SerializeField] bool showTileOutlineFlag = true;
        [SerializeField] bool showTileCenterFlag = true;
        [SerializeField] bool showTileInfoTextFlag = false;
        [SerializeField] bool ShowNodeGriPositionTextFlag = false;
        [SerializeField] bool ShowNodeWalkableTextFlag = false;
        [SerializeField] bool ShowNodeMovementCostTextFlag = false;
        [SerializeField] bool ShowOccupyingActorTextFlag = false;
        [SerializeField] int TileInfoTextFontSize = 10;
        [SerializeField] Color TileInfoTextColor;
#endif
        #endregion

        #region Properties
        public int Width { get => width; protected set => width = value; }
        public int Height { get => height; protected set => height = value; }
        public int Count { get => Height * Width; }
        public Vector3 Position { get => transform.position; }
        public float TileSize { get => tileSize; protected set => tileSize = value; }

#if UNITY_EDITOR
        protected bool DebugDrawTileOutline { get => showTileOutlineFlag; }
        protected bool DebugDrawTileCenter { get => showTileCenterFlag; }
#endif
        #endregion

        //<summary>
        //This method initializes the grid. It creates the node array and configures each node with its index, position, and walkability status.
        //It also configures the map's collision box. 
        //</summary>
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
                    nodeWorldPositions[gridIndex] = GridPositionToWorldPosition(w, h);
                    walkable = TestForWalkability(nodeWorldPositions[gridIndex], notWalkableLayers, colliderSize, rayLength);
                    nodes[gridIndex].Setup(gridIndex, w, h, walkable);
                }
            }

            SetupCollider(collisionLayer);
        }


        protected abstract bool TestForWalkability(Vector3 nodeWorldPosition, LayerMask notWalkableLayers, float colliderSize, float rayLength);


        //<summary>
        // This method sets up the collider
        //</summary>
        protected abstract void SetupCollider(int collisionLayer);


        //<summary>
        // This method scans the scene for Actors. If it finds actors above the grid, it tries to Intall them on the grid
        //</summary>
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

        public int IndexAt(int x, int z)
        {
            if (x >= Width || x < 0 || z >= Height || z < 0)
            {
                return -1;
            }
            return x + z * Width;
        }


        public int IndexAt(Vector2Int p)
        {
            return IndexAt(p.x, p.y);
        }


        public int IndexAt(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldPositionToGridPosition(worldPosition);
            return IndexAt(gridPos.x, gridPos.y);
        }
        #endregion


        #region Node Getters
        public Node NodeAt(int index)
        {
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[index];
        }


        public Node NodeAt(int x, int z)
        {
            int index = IndexAt(x, z);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[IndexAt(x, z)];
        }


        public Node NodeAt(Vector2Int gridPosition)
        {
            int index = IndexAt(gridPosition);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[IndexAt(gridPosition)];
        }


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

        public Vector2Int GridPositionAt(int index)
        {
            int x = index % width;
            int z = index / width;
            return new Vector2Int(x, z);
        }
        #endregion


        public Vector2Int GridPositionAt(Vector3 worldPosition)
        {
            return WorldPositionToGridPosition(worldPosition);
        }


        #region World Position Getters and Conversion
        public Vector3 NodeWorldPositionAt(int x, int z)
        {
            return nodeWorldPositions[IndexAt(x, z)];
        }


        public Vector3 NodeWorldPositionAt(Vector2Int p)
        {
            return nodeWorldPositions[IndexAt(p)];
        }


        public Vector3 NodeWorldPositionAt(int index)
        {
            return nodeWorldPositions[index];
        }

        protected abstract Vector2Int WorldPositionToGridPosition(Vector3 worldPosition);
        protected abstract Vector3 GridPositionToWorldPosition(int x, int z);

        #endregion


        #region Walkability Checkers

        public bool IsWalkable(int index)
        {
            return nodes[index].walkable && actors.ContainsKey(index) == false;
        }


        public bool IsWalkable(int x, int z)
        {
            return IsWalkable(IndexAt(x, z));
        }


        public bool IsWalkable(Vector2Int gridPosition)
        {
            return IsWalkable(IndexAt(gridPosition));
        }

        #endregion


        private void OnDrawGizmos()
        {

            if (showTileInfoTextFlag)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.fontSize = TileInfoTextFontSize;

                Color cachedColor = GUI.color;
                GUI.color = GUI.color = TileInfoTextColor;

                foreach (Node n in nodes)
                {
                    DrawTileInfoText(n, style);
                }

                GUI.color = cachedColor;
            }

            Gizmos.color = Color.white;

            if (nodes == null)
            {
                Gizmos.DrawSphere(transform.position, 1.0f);
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


        }


        protected abstract void DrawNodeCenterOutineGizmos(Node node);

#if UNITY_EDITOR
        protected void DrawTileInfoText(Node node, GUIStyle style)
        {
            bool addSeperator = false;
            string infoText = " ";

            if (ShowNodeGriPositionTextFlag)
            {
                infoText += $"({node.gridPosition.x},{node.gridPosition.y}) ";
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
                    infoText += actors[node.id].gameObject.name;
                }
            }


            Handles.Label(nodeWorldPositions[node.id], infoText, style);
        }
#endif


        #region Bound Checking

        public bool CheckIfInBound(int startX, int startZ)
        {
            if (startX < 0 || startX >= Width || startZ < 0 || startZ >= Height)
            {
                return false;
            }
            return true;
        }


        public bool CheckIfInBound(Vector2Int start)
        {
            if (start.x < 0 || start.x >= Width || start.y < 0 || start.y >= Height)
            {
                return false;
            }
            return true;
        }

        #endregion


        public void SetMovementModifier(int x, int z, float value)
        {
            nodes[IndexAt(x, z)].movementCostModifier = value;
        }

        //<summary>
        // This method places the provided actor on the map at the given node index, registers it and sets it up
        //</summary>
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
            return true;
        }


        //<summary>
        // This method places the provided actor on the map at the given x,y coordinates, registers it and sets it up
        //</summary>
        public bool InstallActor(Actor actor, int x, int z)
        {
            return InstallActor(actor, IndexAt(x, z));
        }


        //<summary>
        // This method places the removes actor refernece on the map at the given node index. 
        // It does not alter the actor fields, thus should be used when actor gets Destoroyed
        //</summary>
        public void RemoveActor(int index)
        {
            actors.Remove(index);
        }


        //<summary>
        // This method places the removes actor refernece on the map at the given node index 
        // and resets the actor
        //</summary>
        public void UninstallActor(int index)
        {
            if (actors.ContainsKey(index))
            {
                Actor actor = actors[index];
                actor.Deinitialize();
            }
            actors.Remove(index);
        }


        public void UninstallAllActors()
        {
            foreach (var pair in actors)
            {
                pair.Value.Deinitialize();
            }
            actors.Clear();
        }


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



    }
}