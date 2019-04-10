using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.timeline
{
    [Action("行为/移动", typeof(MoveAction))]
    public class MoveAction : TimeAction
    {
        [Desc("开始挂点")]
        public IPosition start;
        [Desc("结束挂点")]
        public IPosition end;
        [Desc("运动轨迹")]
        public IEvaluateV3 eva;
        public override TriggerStatus OnTrigger()
        {
            return (start == null || end == null || eva == null) ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            Vector3 pos = eva.Evaluate(this.start.pos, this.end.pos, this.timeObject.progress);
            this.res.obj.SetPos(pos);
        }
    }
}