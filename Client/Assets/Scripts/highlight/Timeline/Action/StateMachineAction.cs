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
        public int curValue;
        public Observer<StateMachineAction> obs_state = new Observer<StateMachineAction>();
        public void AddObserver(AcHandler<StateMachineAction> ac)
        {
            obs_state.AddObserver(ac);
        }
        public void RemoveObserver(AcHandler<StateMachineAction> ac)
        {
            obs_state.RemoveObserver(ac);
        }
        public override void OnInit()
        {
            this.owner.SetStateMachine(data.type, this);
            curValue = this.data.value;
        }
        public override TriggerStatus OnTrigger()
        {
            for (int i = 0; i < this.timeObject.childCount; i++)
            {
                TimeObject obj = this.timeObject.GetChild(i);
                bool isCur = curValue == i;// obj.name;
                if (isCur)
                    obj.Reset();
                else
                    obj.TryStop();
            }
            return TriggerStatus.Success;
        }
        public void Switch(int state)
        {
            if (this.curValue != state)
            {
                TimeObject curObj = this.timeObject.GetChild(curValue);
                curObj.TryStop();
                this.curValue = state;
                TimeObject nextObj = this.timeObject.GetChild(state);
                nextObj.Reset();
                nextObj.Trigger();
            }
        }
        public override void OnUpdate()
        {
        }
        public override void OnDestroy()
        {
            obs_state.Clear();
            base.OnDestroy();
        }
    }
}