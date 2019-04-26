using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/Bool属性执行", typeof(AttrBoolAction))]
    public class AttrBoolAction : TimeAction, IBoolAttrValue
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
        //private BoolAttr attr {
        //    get
        //    {
        //        return obj.GetBoolAttr(data.attrType, true);
        //    }
        //}
        public override bool OnTrigger()
        {
            if (data == null)
                return false;
            obj.attrs.AddBool(this.data.attrType, this);
            //else
            //{
            //attr.Add(this);
            //this.obj.Change(this.data.attrType);
            //}
            return true;
        }
        public override void OnFinish()
        {
            obj.attrs.RemoveBool(this.data.attrType, this);
            //if(attr != null)
            //{
            //    attr.Remove(this);
            //    this.obj.Change(this.data.attrType);
            //}
        }
        public BoolValue GetValue()
        {
            return this.data.value;
        }
    }
}