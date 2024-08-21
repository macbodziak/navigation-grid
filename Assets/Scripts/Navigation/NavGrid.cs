using UnityEngine;
using Utils;

namespace Navigation
{
    public class NavGrid : MonoBehaviour
    {

        [SerializeField] float tileSize;
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] Node[] nodes;

        public int Width { get => width; private set => width = value; }
        public int Height { get => height; private set => height = value; }
        public int Count { get => Height * Width; }
        public Vector3 Position { get => transform.position; }
        public float TileSize { get => tileSize; private set => tileSize = value; }

        private void Start()
        {
            // Debug.Log("Map Start()");

        }


        private void Update()
        {
            // Debug.Log($"{tileSize},{Width},{Height}");
        }


        public void CreateMap(int width, int height, float tileSize, LayerMask notWalkableLayers, float colliderSize, float rayLength)
        {
            this.Width = width;
            this.Height = height;
            this.TileSize = tileSize;

            nodes = new Node[width * height];

            int gridIndex;
            bool walkable;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    gridIndex = IndexAt(w, h);
                    walkable = TestForWalkability(w, h, notWalkableLayers, colliderSize, rayLength);
                    nodes[gridIndex].Setup(gridIndex, w, h, walkable);
                }
            }


            foreach (Node n in nodes)
            {
                Debug.Log(n);
            }
        }


        private bool TestForWalkability(int x, int z, LayerMask notWalkableLayers, float colliderSize, float rayLength)
        {
            Vector3 nodePosition = GetNodeWorldPosition(x, z);
            Vector3 center = nodePosition + new Vector3(0f, rayLength, 0f);
            Vector3 halfExtents = Vector3.one * TileSize * 0.5f * colliderSize;
            Vector3 direction = Vector3.down;
            float maxDistance = rayLength;

            if (Physics.BoxCast(center, halfExtents, direction, Quaternion.identity, maxDistance, notWalkableLayers))
            {
                return false;
            }
            return true;
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


        public Node NodeAt(int index)
        {
            return nodes[index];
        }


        public Node NodeAt(int x, int z)
        {
            return nodes[IndexAt(x, z)];
        }


        public Node NodeAt(Vector2Int gridPosition)
        {
            return nodes[IndexAt(gridPosition)];
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

                Gizmos.DrawCube(GetNodeWorldPosition(n.gridPosition.x, n.gridPosition.y), new Vector3(0.1f, 0.1f, 0.1f));
            }
        }


        public Vector3 GetNodeWorldPosition(int x, int z)
        {
            return new Vector3(transform.position.x + x * TileSize,
                                transform.position.y,
                                transform.position.z + z * TileSize);
        }


        public Vector3 GetNodeWorldPosition(Vector2Int p)
        {
            return GetNodeWorldPosition(p.x, p.y);
        }


        public Vector3 GetNodeWorldPosition(int index)
        {
            return new Vector3(transform.position.x + nodes[index].gridPosition.x * TileSize,
                    transform.position.y,
                    transform.position.z + nodes[index].gridPosition.y * TileSize);
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