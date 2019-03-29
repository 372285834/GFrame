using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.timeline
{
   
    public class TimeStyle
    {
        public TimeFlag eFlag = TimeFlag.Node;
        public int Start = 0;
        public int End = 0;
        //[NonSerialized]
        public List<TimeStyle> Childs = new List<TimeStyle>();
        public List<TimeComponent> Components = new List<TimeComponent>();
        // [JsonIgnore]
        public int Length { set { End = Start + value; } get { return End - Start; } }
        // [JsonIgnore]
        public FrameRange Range
        {
            get { return new FrameRange(Start, End); }
            set { Start = value.Start; End = value.End; }
        }
        public List<TimeStyle> GetChilds()
        {
            return Childs;
        }
        public TimeStyle CreatStyle(Type t)
        {
            TimeStyle evt = Activator.CreateInstance(t) as TimeStyle;
            evt.Range = this.Range;
            return evt;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public static ObjectPool<TimeObject> objPool = new ObjectPool<TimeObject>();
        public virtual TimeObject getObj()
        {
            TimeObject obj = objPool.Get();
            return obj;
        }
        public virtual void release(TimeObject obj)
        {
            objPool.Release(obj);
        }
    }
}