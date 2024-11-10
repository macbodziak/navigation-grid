using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
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

#if UNITY_EDITOR
        [SerializeField] bool showTileOutlineFlag = true;
        [SerializeField] bool showTileCenterFlag = true;
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

        public void CreateMap(int width, int height, float tileSize, LayerMask notWalkableLayers, int collisionLayer, float colliderSize, float rayLength)
        {
            this.Width = width;
            this.Height = height;
            this.TileSize = tileSize;

            nodes = new Node[width * height];
            nodeWorldPositions = new Vector3[width * height];

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

        protected abstract void SetupCollider(int collisionLayer);

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
            return nodes[index].walkable;
            //TO DO - add more checks, such as is occupied by a character
        }


        public bool IsWalkable(int x, int z)
        {
            return nodes[IndexAt(x, z)].walkable;
            //TO DO - add more checks, such as is occupied by a character
        }


        public bool IsWalkable(Vector2Int gridPosition)
        {
            return nodes[IndexAt(gridPosition)].walkable;
            //TO DO - add more checks, such as is occupied by a character
        }

        #endregion



        private void OnDrawGizmos()
        {
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

                DrawNodeGizmos(n);
                DrawTileInfoText(n);
                // Gizmos.DrawCube(NodeWorldPositionAt(n.gridPosition.x, n.gridPosition.y), new Vector3(0.1f, 0.1f, 0.1f));
            }
        }


        protected abstract void DrawNodeGizmos(Node node);

        protected void DrawTileInfoText(Node node)
        {
            string infoText = $" {node.gridPosition.x},{node.gridPosition.y} " + $"{node.walkable} ";

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 10;

            //remember old color
            //
            GUI.color = Color.cyan;

            Handles.Label(nodeWorldPositions[node.id], infoText, style);
        }


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
    }
}