using System;
using UnityEngine;

namespace Navigation
{
    [Serializable]
    public struct Node
    {
        [SerializeField] int _id;
        [SerializeField] Vector2Int _gridPosition;
        [SerializeField] bool _walkable;



        public Node(int id, int x, int z, bool walkable)
        {
            _id = id;
            _gridPosition = new Vector2Int(x, z);
            _walkable = walkable;
        }

        public int id { get => _id; private set => _id = value; }
        public Vector2Int gridPosition { get => _gridPosition; private set => _gridPosition = value; }
        public bool walkable { get => _walkable; private set => _walkable = value; }


        public void Setup(int id, int x, int z, bool walkable = true)
        {
            _id = id;
            _gridPosition = new Vector2Int(x, z);
            _walkable = walkable;
        }

        public override string ToString()
        {
            return $"id:{id}, x:{gridPosition.x}, y:{gridPosition.y}, walkable:{walkable}";
        }
    }

}