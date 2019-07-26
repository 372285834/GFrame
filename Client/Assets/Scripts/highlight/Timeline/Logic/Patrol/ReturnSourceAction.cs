using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/回原点", typeof(ReturnSourceAction))]
    public class ReturnSourceAction : TimeAction
    {
        [Desc("当前坐标")]
        public IVector3 cur;
        public NpcMapData data;
        public override TriggerStatus OnTrigger()
        {
            data = this.owner.data as NpcMapData;
            if (data == null)
                return TriggerStatus.Failure;
            cur.vec3 = data.Pos;
            return (cur == null) ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
        }
    }
}