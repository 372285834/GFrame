using UnityEngine;
using System.Collections;
namespace highlight
{
    [System.Serializable]
    public struct Vector2Int
    {
        public static Vector2Int zero = new Vector2Int(0, 0);
        public static Vector2Int one = new Vector2Int(1, 1);
        public static Vector2Int defeat = new Vector2Int(-1, -1);
        public int x;

        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2Int(Vector2Int other)
        {
            this.x = other.x;
            this.y = other.y;
        }

        public override bool Equals(object obj)
        {
            return !object.ReferenceEquals(null, obj) && obj is Vector2Int && this.Equals((Vector2Int)obj);
        }

        public override int GetHashCode()
        {
            return Vector2Int.GetHashCode(this.x, this.y);
        }

        public static int GetHashCode(int x, int y)
        {
            return x * 397 ^ y;
        }

        public bool Equals(Vector2Int other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", this.x, this.y);
        }

        public static bool operator ==(Vector2Int first, Vector2Int second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Vector2Int first, Vector2Int second)
        {
            return !first.Equals(second);
        }

        public static Vector2Int operator -(Vector2Int first, Vector2Int second)
        {
            return new Vector2Int(first.x - second.x, first.y - second.y);
        }

        public static Vector2Int operator +(Vector2Int first, Vector2Int second)
        {
            return new Vector2Int(first.x + second.x, first.y + second.y);
        }

        public static Vector2Int operator *(Vector2Int first, float val)
        {
            return new Vector2Int((int)((float)first.x * val), (int)((float)first.y * val));
        }

        public static Vector2Int operator /(Vector2Int first, float val)
        {
            return new Vector2Int((int)((float)first.x / val), (int)((float)first.y / val));
        }

        public static Vector2Int operator +(Vector2Int first, int second)
        {
            return new Vector2Int(first.x + second, first.y + second);
        }

        public static Vector2Int operator -(Vector2Int first, int second)
        {
            return new Vector2Int(first.x - second, first.y - second);
        }
    }
}