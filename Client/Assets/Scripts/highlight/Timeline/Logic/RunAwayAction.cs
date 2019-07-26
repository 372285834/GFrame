using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/逃跑", typeof(RunAwayAction))]
    public class RunAwayAction : TimeAction
    {
        [Desc("目标坐标")]
        public IVector3 end;
        public override TriggerStatus OnTrigger()
        {
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            Role target = this.target.getObj(0);
            if (target != null)
            {
                Vector3 dir = this.owner.position - target.position;
                dir.Normalize();
                end.vec3 = this.owner.position + dir * 1000f;
            }
        }
    }
}