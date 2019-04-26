using System.Collections;
using System.Collections.Generic;
namespace highlight.tl
{
    [Action("行为/Int属性执行", typeof(AttrPropAction))]
    public class AttrPropAction : TimeAction,IPropAttrValue
    {
        [Desc("Int数据")]
        public AttrPropData data;
        [Desc("目标")]
        public TargetData target;
        [Desc("间隔时间")]
        public CountData interval;
        public Role obj
        {
            get
            {
                if (target == null)
                    return this.owner;
                return this.target.obj;
            }
        }
        public override bool OnTrigger()
        {
            if (data == null || this.obj == null)
                return false;

            if (data.count <= 0)
            {
                this.obj.attrs.AddProp(this.data.attrType, this);
            }

            return true;
        }
        public override void OnUpdate()
        {
            bool b = true;
            if(interval != null)
            {
                interval.cur += App.deltaFrame;
                if (interval.cur >= interval.count)
                {
                    interval.cur = interval.cur - interval.count;
                    b = true;
                }
                else
                    b = false;
            }
            if(b && data.curCount < data.count)
            {
                data.curCount++;
                this.obj.attrs.CalcProp(this.data.attrType, this.data.value);
            }
        }
        public override void OnFinish()
        {
            if (data.count <= 0)
            {
                obj.attrs.RemoveProp(this.data.attrType, this);
            }
        }
        //public IntValue UpdateValue(IntValue v)
        //{
        //    return IntValue.Calculation(this.data.calcType, this.data.value, v);
        //}
        public PropValue GetValue()
        {
            return this.data.value;
        }
    }
}