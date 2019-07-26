using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    public class ConditionBase : TimeAction
    {
        [Desc("条件_类型")]
        public ConditionBaseData data;
        public virtual TriggerStatus TriggerCheck(bool b)
        {
            TriggerStatus result = b ? TriggerStatus.Success : TriggerStatus.Running;
            if (data != null)
            {
                result = this.data.GetStatus(b);
            }
            return result;
        }
        public override void OnUpdate()
        {
            if (data != null && data.conditionType == ConditionType.永久执行)
                this.status = TriggerStatus.Running;
            base.OnUpdate();
        }
    }

    [Action("条件/条件_执行", typeof(ConditionAction))]
    public class ConditionAction : ConditionBase
    {
        public List<ConditionData> cList = new List<ConditionData>();
        public override void OnInit()
        {
            base.OnInit();
            cList.Clear();
            List<ComponentData> comps = this.timeObject.ComponentList;
            for (int i = 0; i < comps.Count; i++)
            {
                if (comps[i] is ConditionData)
                {
                    ConditionData db = comps[i] as ConditionData;
                    cList.Add((ConditionData)comps[i]);
                    if (data != null && data.isObs)
                    {
                        db.Register(this.OnChange);
                    }
                }
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if(data != null && data.isObs)
            {
                for(int i=0;i<cList.Count;i++)
                {
                    cList[i].Remove();
                }
            }
            cList.Clear();
        }
        void OnChange()
        {
            OnTrigger();
        }
        public override TriggerStatus OnTrigger()
        {
            if (cList.Count == 0)
                return TriggerStatus.Failure;
            bool b = ConditionTool.Check(this.style.key, cList);
            //if(this.timeObject.name == "atk" && data == null)
            //{
            //    Debug.LogError("data == null");
            //}
            int count = this.timeObject.childCount;
            if (count > 0)
            {
                TimeObject obj1 = this.timeObject.GetChild(0), obj2 = null;
                if (count > 1)
                    obj2 = this.timeObject.GetChild(1);
                if (b)
                {
                    obj1.Reset();
                    if (obj2 != null)
                    {
                        obj2.Stop();
                    }
                }
                else
                {
                    obj1.Stop();
                    if (obj2 != null)
                    {
                        obj2.Reset();
                    }
                }
            }
            return TriggerCheck(b);
        }
    }
}