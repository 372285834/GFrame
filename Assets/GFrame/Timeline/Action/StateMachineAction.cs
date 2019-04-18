using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
    [Action("行为/状态机控制", typeof(StateMachineAction))]
    public class StateMachineAction : TimeAction
    {
        [Desc("状态数据")]
        public StateData data;
        public override void OnInit()
        {
           // this.owner.StateMachine = this;
        }
        public override TriggerStatus OnTrigger()
        {
           // this.owner.state = this.data.curState;
            for (int i = 0; i < this.timeObject.childCount; i++)
            {
                TimeObject obj = this.timeObject.GetChild(i);
                bool isCur = data.curState == i;// obj.name;
                if (isCur)
                    obj.Reset();
                else
                    obj.Stop();
            }
            // this.owner.animator
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
        }
        public int curState { get { return this.data.curState; } }
        public StateType stateType { get { return this.data.stateType; } }
        public void Switch(int state)
        {
            if (this.data.curState != state)
            {
                this.data.curState = state;
                TimeObject curObj = this.timeObject.GetChild(this.data.curState);
                curObj.Stop();
                TimeObject nextObj = this.timeObject.GetChild(state);
                nextObj.Reset();
                nextObj.Trigger();
            }
        }
    }
}