using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/寻敌", typeof(FindTargetAction))]
    public class FindTargetAction : TimeAction
    {
        public override TriggerStatus OnTrigger()
        {
            float field = this.owner.attrs.GetFloat(AttrType.field);
            Role r = RoleManager.Find(RoleType.Player, this.owner.position, field);
            if(r != null)
            {
                this.target.setObj(r);
                return TriggerStatus.Success;
            }
            return TriggerStatus.Running;
        }
        public override void OnUpdate()
        {
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}