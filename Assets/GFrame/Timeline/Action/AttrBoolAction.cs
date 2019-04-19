using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/Bool属性执行", typeof(AttrBoolAction))]
    public class AttrBoolAction : TimeAction
    {
        [Desc("Bool数据")]
        public AttrBoolData data;
        [Desc("目标")]
        public TargetData target;
        public Role obj
        {
            get
            {
                if (target == null)
                    return this.owner;
                return this.target.obj;
            }
        }
        private BoolAttr attr;
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            //else
            //{
            attr = this.owner.attrs.GetBoolAttr(data.attrType, true);
            attr.AddObserver(this.CalcBool);
            this.obj.attrs.Change(this.data.attrType);
            //}
            return TriggerStatus.Success;
        }
        public override void OnFinish()
        {
            attr.RemoveObserver(this.CalcBool);
            this.obj.attrs.Change(this.data.attrType);
        }
        public override void OnStop()
        {
            attr = null;
        }
        public BoolValue CalcBool(BoolValue v)
        {
            bool b = this.data.GetBool();
            return new BoolValue(b);
        }
    }
}