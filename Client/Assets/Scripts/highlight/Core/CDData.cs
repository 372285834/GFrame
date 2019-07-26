using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public struct CDData
    {
        public static CDData One = new CDData(1);
        public static CDData Zero = new CDData(0);
        public static CDData Min = new CDData(0,int.MinValue);
        public CDData(int start, int len)
        {
            this.start = start;
            this.length = len;
        }
        public CDData(int len)
        {
            this.start = App.time;
            this.length = len;
        }
        public int start;
        public int length;
        public int curTime { get { return App.time - this.start; } }
        public int endTime { get { return start + length; } }
        public int remindTime { get { return endTime - App.time; } }
        public float progress
        {
            get
            {
                if (length <= 0)
                    return 1f;
                return remindTime / length;
            }
        }
        public bool IsComplete
        {
            get
            {
                if (length == int.MinValue)
                    return false;
                return App.time >= endTime;
            }
        }
        public CDData Reset()
        {
            this.start = App.time;
            return this;
        }
        public CDData Clear()
        {
            length = App.time - this.start;
            return this;
        }
        public static bool operator ==(CDData a, CDData b)
        {
            return a.start == b.start && a.length == b.length;
        }
        public static bool operator !=(CDData a, CDData b)
        {
            return a.start != b.start || a.length != b.length;
        }

        public override bool Equals(object obj)
        {
            return !object.ReferenceEquals(null, obj) && obj is CDData && this.Equals((CDData)obj);
        }

        public override int GetHashCode()
        {
            return endTime;
        }
        
        public bool Equals(CDData other)
        {
            return this.start == other.start && this.length == other.length;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", this.start, this.length, endTime, this.IsComplete);
        }
    }
}