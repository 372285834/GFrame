using UnityEngine;
using System.Collections;
namespace highlight
{
    [System.Serializable]
    public struct RectInt
    {
        public static RectInt zero = new RectInt(0);
		public int left;

		public int up;

		public int right;

		public int down;

        public int width { get { return right - left + 1; } }
        public int height { get { return down - up + 1; } }
        public Vector2Int size { get { return new Vector2Int(this.width, this.height); } }
        public Vector2Int center { get { return new Vector2Int(Mathf.FloorToInt((left + right) * 0.5f), Mathf.FloorToInt((up + down) * 0.5f)); } }
        public RectInt(int v)
        {
            this.left = v;
            this.up = v;
            this.right = v;
            this.down = v;
        }
        public RectInt(int x1, int y1, int x2, int y2)
		{
			this.left = x1;
			this.up = y1;
			this.right = x2;
			this.down = y2;
		}
        public RectInt(int centerX,int centerY,int r,Vector2Int size)
        {
            this.left = Mathf.Clamp(centerX - r, 0, size.x);
            this.right = Mathf.Clamp(centerX + r, 0, size.x);
            this.up = Mathf.Clamp(centerY - r, 0, size.y);
            this.down = Mathf.Clamp(centerY + r, 0, size.y);
        }
        public RectInt(int x,int y,int size)
        {
            this.left = x;
            this.right = x + size;
            this.up = y;
            this.down = y + size;
        }
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
            RectInt mapRect = (RectInt)obj;
			return mapRect.left == this.left && mapRect.right == this.right && mapRect.up == this.up && mapRect.down == this.down;
		}

		public override int GetHashCode()
		{
			return 0;
		}

        public void CopyFrom(RectInt other)
		{
			this.left = other.left;
			this.right = other.right;
			this.up = other.up;
			this.down = other.down;
		}

        public RectInt Clone()
		{
            RectInt mapRect = new RectInt();
			mapRect.CopyFrom(this);
			return mapRect;
		}

		public bool IsInside(int x, int y)
		{
			return this.left <= x && this.up <= y && x <= this.right && y <= this.down;
		}
        public bool IsInsideTight(int x, int y)
        {
            return this.left <= x && this.up <= y && x < this.right && y < this.down;
        }
		public bool IsInside(Vector2Int pos)
		{
			return this.IsInside(pos.x, pos.y);
		}
        /// <summary>
        /// 是否为无效的逆反方块
        /// </summary>
        /// <returns></returns>
        public bool IsInverse()
        {
            return this.left > this.right || this.up > this.down || (left == right && up == down);
        }
        /// <summary>
        /// 获取两个方块的重叠部分
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static RectInt Overlap(RectInt a,RectInt b)
        {
            int fxmin = Mathf.Max(a.left, b.left);
            int fxmax = Mathf.Min(a.right, b.right);
            int fymin = Mathf.Max(a.up, b.up);
            int fymax = Mathf.Min(a.down, b.down);
            return new RectInt(fxmin, fymin, fxmax, fymax);
        }
        public static bool IsOverlap(RectInt a, RectInt b)
        {
            return Overlap(a,b).IsInverse();
        }
		public override string ToString()
		{
            return string.Format("[LU:{0},{1}, RD:{2},{3}]", new object[]
			{
				this.left,
				this.up,
				this.right,
				this.down
			});
		}
        public bool Equals(RectInt other)
        {
            return this.left == other.left && this.right == other.right && this.up == other.up && this.down == other.down;
        }
        public static bool operator ==(RectInt first, RectInt second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(RectInt first, RectInt second)
        {
            return !first.Equals(second);
        }
    }
}