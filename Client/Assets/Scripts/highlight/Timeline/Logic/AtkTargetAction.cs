using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/攻击目标", typeof(AtkTargetAction))]
    public class AtkTargetAction : TimeAction
    {
        public CDData cd = CDData.One;
        public override TriggerStatus OnTrigger()
        {
            this.owner.Switch(RoleState.Idle);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            int atk = this.owner.attrs.GetInt(AttrType.atk_speed,false);
            if (atk <= 0)
                return;
            cd.length = 1000*5000/atk;
            if (cd.IsComplete)
            {
                cd.Reset();
                Role target = this.target.getObj(0);
                Vector3 forward = (target.position - this.owner.position);
                this.owner.SetForward(forward.normalized, true);
                this.owner.Switch(RoleState.Attack, true);
                if(target.CanPlayHit)
                    target.Switch(RoleState.Hit);
               // this.owner.PlayClip("combo1");
            }
            base.OnUpdate();
        }
        //public override void OnStop()
        //{
        //    base.OnStop();
        //}
    }
}