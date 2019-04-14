using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.timeline
{
    [Action("行为/状态机控制", typeof(StateMachineAction))]
    public class StateMachineAction : TimeAction
    {
        [Desc("状态数据")]
        public RoleStateData data;

        public override void OnInit()
        {

        }
        public override TriggerStatus OnTrigger()
        {
            this.owner.state = this.data.curState;
            for (int i = 0; i < this.timeObject.childCount; i++)
            {
                TimeObject obj = this.timeObject.GetChild(i);
                bool isCur = data.curState == obj.name;
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
            if(this.data.curState != this.owner.state)
            {
                TimeObject curObj = this.timeObject.GetChild(this.data.curState);
                curObj.Stop();
                TimeObject nextObj = this.timeObject.GetChild(this.owner.state);
                nextObj.Reset();
                this.data.curState = this.owner.state;
            }
        }
    }
}