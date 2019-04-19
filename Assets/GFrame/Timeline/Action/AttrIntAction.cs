using System.Collections;
using System.Collections.Generic;
namespace highlight.tl
{
    [Action("行为/Int属性执行", typeof(AttrIntAction))]
    public class AttrIntAction : TimeAction
    {
        [Desc("Int数据")]
        public AttrIntData data;
        [Desc("目标")]
        public TargetData target;
        [Desc("间隔时间")]
        public CountData interval;
        private IntAttr attr;
        public Role obj
        {
            get
            {
                if (target == null)
                    return this.owner;
                return this.target.obj;
            }
        }
        public override TriggerStatus OnTrigger()
        {
            if (data == null || this.obj == null || this.obj.attrs == null)
                return TriggerStatus.Failure;

            attr = this.obj.attrs.GetIntAttr(data.attrType, true);
            if (data.count <= 0)
            {
                attr.AddObserver(this.CalcInt);
                this.obj.attrs.Change(this.data.attrType);
            }
                

            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            bool b = true;
            if(interval != null)
            {
                interval.cur++;
                if (interval.cur == interval.count)
                {
                    interval.cur = 0;
                    b = true;
                }
                else
                    b = false;
            }
            if(b && data.curCount < data.count)
            {
                data.curCount++;
                attr.value += this.data.value;
                this.obj.attrs.Change(this.data.attrType);
            }
        }
        public override void OnFinish()
        {
            if (data.count <= 0)
            {
                attr.RemoveObserver(this.CalcInt);
                this.obj.attrs.Change(this.data.attrType);
            }
            attr = null;
        }
        public override void OnStop()
        {
            attr = null;
        }
        //public IntValue UpdateValue(IntValue v)
        //{
        //    return IntValue.Calculation(this.data.calcType, this.data.value, v);
        //}
        public IntValue CalcInt(IntValue v)
        {
            v += this.data.value;
            return this.data.value;
        }
    }
}