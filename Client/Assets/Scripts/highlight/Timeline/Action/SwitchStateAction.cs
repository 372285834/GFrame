using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    [Action("行为/状态切换", typeof(SwitchStateAction))]
    public class SwitchStateAction : TimeAction
    {
        [Desc("状态数据")]
        public StateData state;
        public override TriggerStatus OnTrigger()
        {
           // if (!state.IsFinish)
          //  {
                Switch();
                return TriggerStatus.Failure;
           // }
           // return TriggerStatus.Running;
        }

        //public override void OnFinish()
        //{
        //    if (state.IsFinish)
        //        Switch();
        //    base.OnFinish();
        //}
        void Switch()
        {
            StateMachineAction machine = this.owner.GetState(state.type);
            if (machine == null)
            {
                Debug.LogError("machine == null:" + this.name);
            }
            machine.Switch(state.value);
        }
    }
}