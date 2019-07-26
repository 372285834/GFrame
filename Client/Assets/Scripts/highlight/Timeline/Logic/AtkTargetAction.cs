using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/攻击目标", typeof(AtkTargetAction))]
    public class AtkTargetAction : TimeAction
    {
        public CDData cd = CDData.One;
        //public override TriggerStatus OnTrigger()
        //{
        //    return TriggerStatus.Success;
        //}
        public override void OnUpdate()
        {
            int atk = this.owner.attrs.GetInt(AttrType.atk_speed,false,1);
            cd.length = 1000*5000/atk;
            if (cd.IsComplete)
            {
                cd.Reset();
                Vector3 forward = (this.target.getObj(0).position - this.owner.position);
                this.owner.SetForward(forward.normalized);
                this.owner.PlayClip("combo1");
            }
            base.OnUpdate();
        }
        //public override void OnStop()
        //{
        //    base.OnStop();
        //}
    }
}