using System.Collections;
using System.Collections.Generic;
namespace highlight.tl
{
    [Action("行为/数值属性执行", typeof(AttrIntAction))]
    public class AttrIntAction : TimeAction
    {
        [Desc("数值数据")]
        public AttrIntData data;
        private IntAttr attr;
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;

            attr = this.owner.attrs.GetIntAttr(data.attrType);
            if (data.once)
                attr.value += this.data.value;
            else
                attr.AddObserver(this.CalcInt);

            return TriggerStatus.Success;
        }

        public override void OnUpdate()
        {

        }
        public override void OnFinish()
        {
            if (!data.once)
                attr.RemoveObserver(this.CalcInt);
            attr = null;
        }

        //public IntValue UpdateValue(IntValue v)
        //{
        //    return IntValue.Calculation(this.data.calcType, this.data.value, v);
        //}
        public IntValue CalcInt(IntValue v)
        {
            this.data.value += v;
            return this.data.value;
        }
    }
}