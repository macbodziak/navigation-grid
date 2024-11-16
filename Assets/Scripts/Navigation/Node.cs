using System;
using UnityEngine;

namespace Navigation
{
    [Serializable]
    public struct Node
    {
        [SerializeField] int _id;
        [SerializeField] Vector2Int _gridCoordinates;
        [SerializeField] bool _walkable;
        [SerializeField] float _movementCostModifier;



        public Node(int id, int x, int z, bool walkable)
        {
            _id = id;
            _gridCoordinates = new Vector2Int(x, z);
            _walkable = walkable;
            _movementCostModifier = 1.0f;
        }


        public int id { get => _id; private set => _id = value; }
        public Vector2Int gridCoordinates { get => _gridCoordinates; private set => _gridCoordinates = value; }
        public bool walkable { get => _walkable; private set => _walkable = value; }
        public float movementCostModifier { get => _movementCostModifier; set => _movementCostModifier = value; }


        public void Setup(int id, int x, int z, bool walkable = true)
        {
            _id = id;
            _gridCoordinates = new Vector2Int(x, z);
            _walkable = walkable;
            _movementCostModifier = 1.0f;
        }

        public override string ToString()
        {
            return $"id:{id}, x:{gridCoordinates.x}, y:{gridCoordinates.y}, walkable:{walkable}";
        }


        public static Node NullNode()
        {
            Node nullNode = new Node();
            nullNode.id = -1;
            return nullNode;
        }
    }

}