using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Navigation
{
    public class Map : MonoBehaviour
    {

        [SerializeField] float tileSize;
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] Node[] nodes;


        private void Start()
        {
            Debug.Log("Map Start()");
        }


        private void Update()
        {
            Debug.Log($"{tileSize},{width},{height}");
        }


        public void CreateMap(int width, int height, float tileSize, LayerMask notWalkableLayers)
        {
            this.width = width;
            this.height = height;
            this.tileSize = tileSize;

            nodes = new Node[width * height];

            int gridIndex;
            bool walkable;
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    gridIndex = GridIndex(w, h);
                    walkable = CheckIfWalkable(w, h, notWalkableLayers);
                    nodes[gridIndex].Setup(gridIndex, w, h, walkable);
                }
            }


            foreach (Node n in nodes)
            {
                Debug.Log(n);
            }
        }


        private bool CheckIfWalkable(int x, int z, LayerMask notWalkableLayers)
        {
            Vector3 nodePosition = GetNodeWorldPosition(x, z);
            Vector3 center = nodePosition + new Vector3(0f, 100f, 0f);
            Vector3 halfExtents = Vector3.one * tileSize * 0.45f;
            Vector3 direction = Vector3.down;
            float maxDistance = 100f;

            if (Physics.BoxCast(center, halfExtents, direction, Quaternion.identity, maxDistance, notWalkableLayers))
            {
                return false;
            }
            return true;
        }


        private int GridIndex(int x, int z)
        {
            if (x >= width || x < 0 || z >= height || z < 0)
            {
                return -1;
            }
            return x + z * width;
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

                Gizmos.DrawCube(GetNodeWorldPosition(n.x, n.z), new Vector3(0.1f, 0.1f, 0.1f));
            }
        }


        private Vector3 GetNodeWorldPosition(int x, int z)
        {
            return new Vector3(transform.position.x + x * tileSize,
                                transform.position.y,
                                transform.position.z + z * tileSize);
        }
    }
}