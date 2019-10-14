using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/Bool属性执行", typeof(AttrBoolAction))]
    public class AttrBoolAction : TargetAction
    {
        [Desc("Bool数据")]
        public AttrBoolData data;
        //private BoolAttr attr {
        //    get
        //    {
        //        return obj.GetBoolAttr(data.attrType, true);
        //    }
        //}
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            role.attrs.AddBool(this.data.attrType, this.data.value);
            //else
            //{
            //attr.Add(this);
            //this.obj.Change(this.data.attrType);
            //}
            return TriggerStatus.Success;
        }
        public bool IsFinishRemove
        {
            get
            {
                return data.value.cd == CDData.Min;
            }
        }
        public override void OnFinish()
        {
            if(IsFinishRemove)
                role.attrs.RemoveBool(this.data.attrType, this.data.value.id);
        }
        //public BoolValue GetValue()
        //{
        //    return this.data.value;
        //}
    }
}