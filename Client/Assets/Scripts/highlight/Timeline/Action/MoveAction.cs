using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/移动", typeof(MoveAction))]
    public class MoveAction : TargetAction
    {
        [Desc("开始挂点")]
        public IVector3 start;
        [Desc("结束挂点")]
        public IVector3 end;
        [Desc("运动轨迹")]
        public IEvaluateV3 eva;
        public override TriggerStatus OnTrigger()
        {
            return end == null ? TriggerStatus.Failure : TriggerStatus.Failure;
        }
        public override void OnUpdate()
        {
            Vector3 sPos = start == null ? this.owner.position : this.start.vec3;
            Vector3 pos;
            if (eva == null)
            {
                float speed = this.owner.attrs.GetFloat(AttrType.move_speed, false);
                pos = VectorTools.LerpSpeed(sPos, end.vec3, speed * App.logicDeltaTime);
            }
            else
                pos = eva.Evaluate(this.start.vec3, this.end.vec3, this.timeObject.progress);
            this.role.SetPos(pos,false);
        }
    }
}