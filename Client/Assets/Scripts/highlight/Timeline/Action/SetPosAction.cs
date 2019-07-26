using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    [Action("行为/设置坐标", typeof(SetPosAction))]
    public class SetPosAction : TargetAction
    {
        [Desc("目标挂点")]
        public IVector3 target;
        public override TriggerStatus OnTrigger()
        {
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            this.role.SetPos(this.target.vec3,true);
        }
    }
}