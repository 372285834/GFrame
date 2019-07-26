using highlight.tl;
using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public class Buff : Object
    {
        public Role ower;
        public Timeline timeline;
        public TimelineStyle style { get { return timeline != null ? timeline.lStyle : null; } }
        public bool IsStop { get { return timeline != null ? timeline.IsStopped : false; } }
        public void UpdateFrame(int delta)
        {
            if(timeline != null)
                timeline.UpdateFrame(delta);
        }
        private readonly static ObjectPool<Buff> pool = new ObjectPool<Buff>();
        public static Buff Get(Buffs _ower,TimelineStyle _style)
        {
            Buff buff = pool.Get();
            buff.ower = _ower.obj;
            if(_style != null)
            {
              //  Timeline tl = TimelineFactory.Creat(_style, _ower.obj);
             //   buff.timeline = tl;
             //   tl.buff = buff;
            }
            return buff;
        }
        public static void Release(Buff buff)
        {
            if (buff == null)
                return;
            if(buff.timeline != null)
            {
                buff.timeline.Destroy();
                buff.timeline = null;
            }
            buff.ower = null;
            pool.Release(buff);
        }
    }
    public class Buffs : List<Buff>
    {
        public Role obj;
        
        public void AddBuff(TimelineStyle style)
        {
            Buff buff = Buff.Get(this, style);
            base.Add(buff);
        }
        public void RemoveBuff(Buff buff)
        {
            base.Remove(buff);
            Buff.Release(buff);
        }
        static List<Buff> temp = new List<Buff>();
        public void UpdateFrame(int delta)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].UpdateFrame(delta);
                if (this[i].IsStop)
                    temp.Add(this[i]);
            }
            for (int i = 0; i < temp.Count; i++)
            {
                RemoveBuff(temp[i]);
            }
            temp.Clear();
        }
        private readonly static ObjectPool<Buffs> pool = new ObjectPool<Buffs>();
        public static Buffs Get(Role _obj)
        {
            Buffs bfs = pool.Get();
            bfs.obj = _obj;
            return bfs;
        }
        public void Release()
        {
            for (int i = 0; i < this.Count; i++)
            {
                Buff.Release(this[i]);
            }
            base.Clear();
            obj = null;
            pool.Release(this);
        }
    }
}