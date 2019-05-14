using System;
using UnityEngine;

namespace Game.MapStepping
{
    [Serializable]
    public class MyVector3Int
    {
        [SerializeField]
        public int x = 0;
        [SerializeField]
        public int y = 0;
        [SerializeField]
        public int z = 0;

        public MyVector3Int(int X, int Y, int Z)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
        }

        public static MyVector3Int zero
        {
            get { return new MyVector3Int(0, 0, 0); }
        }

        public static MyVector3Int one
        {
            get { return new MyVector3Int(1, 1, 1); }
        }

        public static MyVector3Int operator *(MyVector3Int c1, MyVector3Int c2)
        {
            return new MyVector3Int(c1.x * c2.x, c1.y * c2.y, c1.z * c2.z);
        }

        public static MyVector3Int operator *(MyVector3Int c1, int c2)
        {
            return new MyVector3Int(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public override string ToString()
        {
            return string.Format("(x:{0}, y:{1}, z:{2})", x, y, z);
        }
    }

    public static class Extend
    {
        public static MyVector3Int ToMyVector3Int(this Vector3Int v3Int)
        {
            return new MyVector3Int(v3Int.x, v3Int.y, 0);
        }

        public static Vector3Int ToVector3Int(this MyVector3Int v3Int)
        {
            return new Vector3Int(v3Int.x, v3Int.y, 0);
        }
    }
}
