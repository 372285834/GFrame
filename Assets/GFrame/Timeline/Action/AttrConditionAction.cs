using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/条件属性执行", typeof(AttrConditionAction))]
    public class AttrConditionAction : TimeAction
    {
        [Desc("条件数据")]
        public ICondition data;

        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            //else
            //{
            //    BoolAttr att = this.owner.attrs.GetBoolAttr(data.attrType);
            //    att.AddObserver(this.)
            //}
            return TriggerStatus.Success;
        }

        public BoolValue CalcBool(BoolValue v)
        {
            v.value &= this.data.GetBool();
            return v;
        }
    }
}