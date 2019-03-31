using System;

namespace highlight
{
    public abstract class ActionStyle : ComponentStyle
    {
        public int[] Indexs;
    }
    public abstract class TimeAction : ComponentData
    {
        public virtual void OnUpdate() { }
        public int this[int idx]
        {
            get
            {
                int[] idxs = (this.style as ActionStyle).Indexs;
                return idxs[idx];
            }
        }
        public ComponentData GetComponent(int idx) 
        {
            return this.timeObject.GetComponent(this[idx]);
        }

    }
}