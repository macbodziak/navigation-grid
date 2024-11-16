using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Navigation
{
    public class SquareGrid : NavGrid
    {
        #region Fields
        static private readonly int DIAGONAL_COST = 14;
        static private readonly int STRAIGHT_COST = 10;
        static private readonly Vector2Int[] neighbours = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
        };
        #endregion

        #region Properties
        public Vector2Int[] Neighbours { get => neighbours; }
        public static int DiagonalCost { get => DIAGONAL_COST; }
        public static int StraightCost { get => STRAIGHT_COST; }
        #endregion

        protected override bool TestForWalkability(Vector3 nodeWorldPosition, LayerMask notWalkableLayers, float colliderSize, float rayLength)
        {
            Vector3 center = nodeWorldPosition + new Vector3(0f, rayLength, 0f);
            Vector3 halfExtents = Vector3.one * TileSize * 0.5f * colliderSize;
            Vector3 direction = Vector3.down;
            float maxDistance = rayLength;

            if (Physics.BoxCast(center, halfExtents, direction, Quaternion.identity, maxDistance, notWalkableLayers))
            {
                return false;
            }
            return true;
        }


        protected override void SetupCollider(int collisionLayer)
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
            boxCollider.size = new Vector3(Width * TileSize, 0.1f, Height * TileSize);
            boxCollider.center = new Vector3((Width - 1f) * TileSize * 0.5f, 0f, (Height - 1f) * TileSize * 0.5f);
        }

        protected override Vector2Int WorldPositionToGridCoordinates(Vector3 worldPosition)
        {
            int x = (int)((worldPosition.x - transform.position.x) / TileSize + 0.5f);
            int z = (int)((worldPosition.z - transform.position.z) / TileSize + 0.5f);
            return new Vector2Int(x, z);
        }

        protected override Vector3 GridCoordinatesToWorldPosition(int x, int z)
        {
            return new Vector3(transform.position.x + x * TileSize,
                    transform.position.y,
                    transform.position.z + z * TileSize);
        }

        protected override void DrawNodeCenterOutineGizmos(Node n)
        {
            Vector3 worldPos = nodeWorldPositions[n.id];

            if (DebugDrawTileCenter)
            {
                Gizmos.DrawCube(nodeWorldPositions[n.id], new Vector3(0.1f, 0.1f, 0.1f));
            }

            if (DebugDrawTileOutline)
            {
                Vector3[] points = new Vector3[4];
                points[0] = new Vector3(worldPos.x - TileSize * 0.49f, worldPos.y, worldPos.z + TileSize * 0.49f);
                points[1] = new Vector3(worldPos.x + TileSize * 0.49f, worldPos.y, worldPos.z + TileSize * 0.49f);
                points[2] = new Vector3(worldPos.x + TileSize * 0.49f, worldPos.y, worldPos.z - TileSize * 0.49f);
                points[3] = new Vector3(worldPos.x - TileSize * 0.49f, worldPos.y, worldPos.z - TileSize * 0.49f);
                Gizmos.DrawLineStrip(points, true);
            }
        }
    }
}