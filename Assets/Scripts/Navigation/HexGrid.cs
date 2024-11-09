using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Navigation
{
    public class HexGrid : MonoBehaviour
    {
        #region Fields
        [SerializeField] float tileSize;
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] Node[] nodes;
        [SerializeField] Vector3[] nodeWorldPositions;

#if UNITY_EDITOR
        [SerializeField] bool debugDrawHexes = true;
#endif
        static private readonly int MOVEMENT_COST = 10;
        static private readonly Vector2Int[] neighbours = {
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
        };

        static private readonly float HEX_HEIGHT = 1.15470054F;
        static private readonly float VERTICAL_SPACING = 0.8660254f;

        #endregion

        public int Width { get => width; private set => width = value; }
        public int Height { get => height; private set => height = value; }
        public int Count { get => Height * Width; }
        public Vector3 Position { get => transform.position; }
        public float TileSize { get => tileSize; private set => tileSize = value; }

        public Vector2Int[] Neighbours { get => neighbours; }

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
            foreach (Node n in nodes)
            {
                Debug.Log(n);
            }
        }


        private bool TestForWalkability(Vector3 nodeWorldPosition, LayerMask notWalkableLayers, float colliderSize, float rayLength)
        {
            Vector3 center = nodeWorldPosition + new Vector3(0f, rayLength, 0f);
            Vector3 direction = Vector3.down;
            Ray ray = new Ray(center, direction);
            float radius = TileSize * 0.5f * colliderSize;
            float maxDistance = rayLength;

            if (Physics.SphereCast(ray, radius, maxDistance, notWalkableLayers))
            {
                return false;
            }
            return true;
        }


        private void SetupCollider(int collisionLayer)
        {
            //check if there are already colliders attached and remove them
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                Debug.Log("<color=#ffa500ff>removing existing NavGrid collider</color>: " + col);
                DestroyImmediate(col);
            }

            gameObject.layer = collisionLayer;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3((width + 0.5f) * tileSize, 0.1f, (height - 1) * tileSize * VERTICAL_SPACING + HEX_HEIGHT);
            boxCollider.center = new Vector3((width - 0.5f) * tileSize * 0.5f, 0f, (height - 1) * tileSize * VERTICAL_SPACING * 0.5f);
        }


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
            // TO DO
            Vector2Int gridPos = WorldPositionToGridPosition(worldPosition);
            return IndexAt(gridPos.x, gridPos.y);
        }


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
            return nodes[index];
        }


        public Node NodeAt(Vector2Int gridPosition)
        {
            int index = IndexAt(gridPosition);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[index];
        }

        public Node NodeAt(Vector3 worldPosition)
        {
            int index = IndexAt(worldPosition);
            if (index < 0 || index >= width * height)
            {
                return Node.NullNode();
            }
            return nodes[index];
        }


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

                Vector3 worldPos = nodeWorldPositions[n.id];
                Gizmos.DrawCube(nodeWorldPositions[n.id], new Vector3(0.1f, 0.1f, 0.1f));

                if (debugDrawHexes)
                {
                    Vector3[] points = new Vector3[6];
                    points[0] = new Vector3(worldPos.x, 0f, worldPos.z + tileSize * HEX_HEIGHT * 0.5f);
                    points[1] = new Vector3(worldPos.x + tileSize * 0.5f, 0f, worldPos.z + tileSize * HEX_HEIGHT * 0.25f);
                    points[2] = new Vector3(worldPos.x + tileSize * 0.5f, 0f, worldPos.z + tileSize * HEX_HEIGHT * -0.25f);
                    points[3] = new Vector3(worldPos.x, 0f, worldPos.z + tileSize * HEX_HEIGHT * -0.5f);
                    points[4] = new Vector3(worldPos.x + tileSize * -0.5f, 0f, worldPos.z + tileSize * HEX_HEIGHT * -0.25f);
                    points[5] = new Vector3(worldPos.x + tileSize * -0.5f, 0f, worldPos.z + tileSize * HEX_HEIGHT * 0.25f);
                    Gizmos.DrawLineStrip(points, true);
                }
            }
        }


        public Vector3 GetNodeWorldPosition(int x, int z)
        {
            return nodeWorldPositions[IndexAt(x, z)];
        }

        public Vector3 GridPositionToWorldPosition(int x, int z)
        {
            float worldX = transform.position.x + x * TileSize + z % 2 * TileSize * 0.5f;
            float worldZ = transform.position.z + z * TileSize * VERTICAL_SPACING;
            return new Vector3(worldX, transform.position.y, worldZ);
        }


        public Vector3 GetNodeWorldPosition(Vector2Int p)
        {
            return GetNodeWorldPosition(p.x, p.y);
        }


        public Vector3 GetNodeWorldPosition(int index)
        {
            return nodeWorldPositions[index];
        }

        public Vector2Int WorldPositionToGridPosition(Vector3 worldPosition)
        {

            float GridSpacePositionX = worldPosition.x - transform.position.x;
            float GridSpacePositionZ = worldPosition.z - transform.position.z;

            float q = (0.57735027f * GridSpacePositionX - 0.33333333f * GridSpacePositionZ) / (HEX_HEIGHT * tileSize * 0.5f) + 0.5f;
            float r = 0.66666667f * GridSpacePositionZ / (HEX_HEIGHT * tileSize * 0.5f) + 0.5f;

            float x = q + (r - r % 2) * 0.5f;
            float z = r;

            return new Vector2Int((int)x, (int)z);
        }

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
    }
}