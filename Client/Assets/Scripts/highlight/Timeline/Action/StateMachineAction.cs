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
        public int curValue = -1;
        public int lastValue = -1;
        //public bool enabled = true;
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
          //  if (!enabled)
         //       return TriggerStatus.Running;
            if (this.status == TriggerStatus.InActive)
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
            }
            TimeObject curObj = this.timeObject.GetChild(curValue);
            curObj.UpdateFrame(0);
            return TriggerStatus.Running;
        }
        public void Switch(int state, bool reset = false)
        {
      //      if (!enabled)
      //          return;
            if (this.curValue != state)
            {
                lastValue = this.curValue;
                TimeObject curObj = this.timeObject.GetChild(curValue);
                curObj.TryStop();
                this.curValue = state;
                TimeObject nextObj = this.timeObject.GetChild(state);
                nextObj.Reset();
                nextObj.Trigger();
                obs_state.Change(this);
            }
            else if(reset)
            {
                TimeObject curObj = this.timeObject.GetChild(curValue);
                curObj.TryStop();
                curObj.Reset();
                curObj.Trigger();
                obs_state.Change(this);
            }
        }
        public override void OnStop()
        {
            curValue = -1;
            lastValue = -1;
            base.OnStop();
        }
        public override void OnDestroy()
        {
            obs_state.Clear();
            base.OnDestroy();
        }

        public RoleState role_state
        {
            get
            {
                return (RoleState)this.curValue;
            }
        }
        public Npc_AI_1 npc_ai_state
        {
            get
            {
                return (Npc_AI_1)this.curValue;
            }
        }
        public T Get<T>()
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            return (T)Enum.ToObject(enumType, this.curValue);
        }
    }
}