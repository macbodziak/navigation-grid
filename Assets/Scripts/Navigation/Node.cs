namespace Navigation
{

    public struct Node
    {
        int _id;
        int _x;
        int _z;
        bool _walkable;



        public Node(int id, int x, int z, bool walkable)
        {
            _id = id;
            _x = x;
            _z = z;
            _walkable = walkable;
        }

        public int id { get => _id; private set => _id = value; }
        public int x { get => _x; private set => _x = value; }
        public int z { get => _z; private set => _z = value; }
        public bool walkable { get => _walkable; private set => _walkable = value; }


        public void Setup(int id, int x, int z, bool walkable = true)
        {
            _id = id;
            _x = x;
            _z = z;
            _walkable = walkable;
        }

        public override string ToString()
        {
            return $"id:{id}, x:{x}, y:{z}, walkable:{walkable}";
        }
    }

}