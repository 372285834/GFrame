using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
    [Action("行为/状态切换", typeof(SwitchStateAction))]
    public class SwitchStateAction : TimeAction
    {
        [Desc("状态数据")]
        public StateData state;
        public override bool OnTrigger()
        {
            string key = state.stateType.ToString();
            TimeAction ac = this.root.FindAction(key);
            if(ac is StateMachineAction)
            {
                (ac as StateMachineAction).Switch(state.curState);
                //return TriggerStatus.Success;
            }
            return true;
        }
    }
}