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
            // CreateMap(width, height);
        }


        private void Update()
        {
            Debug.Log(nodes.Count());
        }


        public void CreateMap(int width, int height)
        {
            nodes = new Node[width * height];

            int gridIndex;
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    gridIndex = GridIndex(w, h);
                    // Debug.Log($"{w},{h}:{gridIndex}");
                    nodes[gridIndex].Setup(gridIndex, w, h, true);
                }
            }


            foreach (Node n in nodes)
            {
                Debug.Log(n);
            }
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