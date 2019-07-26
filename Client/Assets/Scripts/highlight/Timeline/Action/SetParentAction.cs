using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    [Action("行为/设置父节点", typeof(SetParentAction))]
    public class SetParentAction : TargetAction
    {
        [Desc("目标挂点")]
        public ITransform target;

        public override TriggerStatus OnTrigger()
        {
            this.role.SetParent(this.target.transform);
            //this.res.obj.SetLocalPos(this.target.loStyle.off);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
        }
    }
}