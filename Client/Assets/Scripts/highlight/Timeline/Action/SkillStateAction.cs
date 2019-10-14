using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/技能/僵直状态_不可打断", typeof(SkillStateAction))]
    public class SkillStateAction : AttrBoolAction
    {
        public override TriggerStatus OnTrigger()
        {
        //    this.role.mainStateMachine.enabled = false;
            this.role.Switch(RoleState.Skill);
            return base.OnTrigger();
        }
        public override void OnFinish()
        {
       //     this.role.mainStateMachine.enabled = true;
             this.role.Switch(RoleState.Idle);
            base.OnFinish();
        }
    }

    [Action("行为/技能/受击状态", typeof(SkillState_Hit_Action))]
    public class SkillState_Hit_Action : AttrBoolAction
    {
        public override TriggerStatus OnTrigger()
        {
            this.role.Switch(RoleState.Hit, true);
            return base.OnTrigger();
        }

        public override void OnFinish()
        {
            this.role.Switch(RoleState.Idle);
            base.OnFinish();
        }
    }
}