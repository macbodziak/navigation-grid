using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Navigation
{
    public class HexGrid : NavGrid
    {
        #region Fields

        static private readonly int MOVEMENT_COST = 10;
        static private readonly Vector2Int[] neighboursOddRow = {
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
        };

        static private readonly Vector2Int[] neighboursEvenRow = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
        };

        static private readonly float HEX_HEIGHT = 1.15470054F;
        static private readonly float VERTICAL_SPACING = 0.8660254f;

        #endregion

        #region Properties
        public Vector2Int[] NeighboursOdd { get => neighboursOddRow; }
        public Vector2Int[] NeighboursEven { get => neighboursEvenRow; }
        public int MovementCost { get => MOVEMENT_COST; }
        #endregion



        protected override bool TestForWalkability(Vector3 nodeWorldPosition, LayerMask notWalkableLayers, float colliderSize, float rayLength)
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
            boxCollider.size = new Vector3((Width + 0.5f) * TileSize, 0.1f, (Height - 1) * TileSize * VERTICAL_SPACING + HEX_HEIGHT);
            boxCollider.center = new Vector3((Width - 0.5f) * TileSize * 0.5f, 0f, (Height - 1) * TileSize * VERTICAL_SPACING * 0.5f);
        }





        protected override Vector3 GridCoordinatesToWorldPosition(int x, int z)
        {
            float worldX = transform.position.x + x * TileSize + z % 2 * TileSize * 0.5f;
            float worldZ = transform.position.z + z * TileSize * VERTICAL_SPACING;
            return new Vector3(worldX, transform.position.y, worldZ);
        }


        protected override Vector2Int WorldPositionToGridCoordinates(Vector3 worldPosition)
        {

            float GridSpacePositionX = worldPosition.x - transform.position.x;
            float GridSpacePositionZ = worldPosition.z - transform.position.z;

            float q = (0.57735027f * GridSpacePositionX - 0.33333333f * GridSpacePositionZ) / (HEX_HEIGHT * TileSize * 0.5f) + 0.5f;
            float r = 0.66666667f * GridSpacePositionZ / (HEX_HEIGHT * TileSize * 0.5f) + 0.5f;

            float x = q + (r - r % 2) * 0.5f;
            float z = r;

            return new Vector2Int((int)x, (int)z);
        }

#if UNITY_EDITOR
        protected override void DrawNodeCenterOutineGizmos(Node n)
        {
            Vector3 worldPos = nodeWorldPositions[n.id];

            if (DebugDrawTileCenter)
            {
                Gizmos.DrawCube(nodeWorldPositions[n.id], new Vector3(0.1f, 0.1f, 0.1f));
            }

            if (DebugDrawTileOutline)
            {
                Vector3[] points = new Vector3[6];
                points[0] = new Vector3(worldPos.x, worldPos.y, worldPos.z + TileSize * HEX_HEIGHT * 0.49f);
                points[1] = new Vector3(worldPos.x + TileSize * 0.49f, worldPos.y, worldPos.z + TileSize * HEX_HEIGHT * 0.245f);
                points[2] = new Vector3(worldPos.x + TileSize * 0.49f, worldPos.y, worldPos.z + TileSize * HEX_HEIGHT * -0.245f);
                points[3] = new Vector3(worldPos.x, worldPos.y, worldPos.z + TileSize * HEX_HEIGHT * -0.49f);
                points[4] = new Vector3(worldPos.x + TileSize * -0.49f, worldPos.y, worldPos.z + TileSize * HEX_HEIGHT * -0.245f);
                points[5] = new Vector3(worldPos.x + TileSize * -0.49f, worldPos.y, worldPos.z + TileSize * HEX_HEIGHT * 0.245f);
                Gizmos.DrawLineStrip(points, true);
            }
        }

        public override List<int> AdjacentNodeIndexes(int index)
        {
            List<int> result = new();
            Vector2Int[] neighbours;

            Vector2Int currentCoordinates = GridCoordinatesAt(index);
            if (currentCoordinates.y % 2 == 0)
            {
                neighbours = neighboursEvenRow;
            }
            else
            {
                neighbours = neighboursOddRow;
            }

            for (int i = 0; i < 6; i++)
            {
                if (CheckIfInBound(currentCoordinates + neighbours[i]))
                {
                    result.Add(IndexAt(currentCoordinates + neighbours[i]));
                }
            }

            return result;
        }
#endif
    }
}