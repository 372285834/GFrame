using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    [Action("行为/Int属性执行", typeof(AttrPropAction))]
    public class AttrPropAction : TargetAction
    {
        [Desc("Int数据")]
        public AttrPropData data;
        [Desc("间隔时间")]
        public IntervalData interval;
        public override TriggerStatus OnTrigger()
        {
            if (data == null || this.role == null)
                return TriggerStatus.Failure;

            if (data.isBuff)
            {
                AddBuff(role, data);
            }

            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            Calculation(role, data, interval);
        }
        public override void OnFinish()
        {
            if (data.isBuff)
            {
                RemoveBuff(role, data);
            }
        }
        //public IntValue UpdateValue(IntValue v)
        //{
        //    return IntValue.Calculation(this.data.calcType, this.data.value, v);
        //}
        //public PropValue GetValue()
        //{
        //    return this.data.value;
        //}

        public static void Calculation(Role role, AttrPropData data, IntervalData interval)
        {
            if (interval != null && interval.OnCheck())
            {
                if (data.isBuff)
                {
                    role.attrs.AddProp(data.attrType, data.value);
                }
                else
                {
                    role.attrs.SetProp(data.attrType, data.value, true);
                }
            }
        }
        public static void AddBuff(Role role, AttrPropData data)
        {
            role.attrs.AddProp(data.attrType, data.value);
        }
        public static void RemoveBuff(Role role, AttrPropData data)
        {
            if(data.value.cd == CDData.Min)
                role.attrs.RemoveProp(data.attrType, data.value.id);
        }
    }


    [Action("行为/Int属性执行_范围_持续", typeof(AttrProp_Rang_Action))]
    public class AttrProp_Rang_Action : TimeAction
    {
        [Desc("Int数据")]
        public AttrPropData data;
        [Desc("间隔时间")]
        public IntervalData interval;
        public override TriggerStatus OnTrigger()
        {
            return data == null ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            if (data.isBuff)//新进入目标 触发
            {
                List<int> inIds = this.target.inObjects;
                for (int i = 0; i < inIds.Count; i++)
                {
                    Role role = RoleManager.Get(inIds[i]);
                    AttrPropAction.AddBuff(role, data);
                }
            }
            if(interval != null)//当前目标 间隔多次触发
            {
                List<int> curIds = this.target.mObjects;
                for (int i = 0; i < curIds.Count; i++)
                {
                    Role role = RoleManager.Get(curIds[i]);
                    AttrPropAction.Calculation(role, data, interval);
                }
            }
            if (data.isBuff)//退出目标 触发
            {
                List<int> outIds = this.target.outObjects;
                for (int i = 0; i < outIds.Count; i++)
                {
                    Role role = RoleManager.Get(outIds[i]);
                    AttrPropAction.RemoveBuff(role, data);
                }
            }
        }
        public override void OnFinish()
        {
          //  Debug.Log("OnFinish:" + this.root.target.mObjects.Count + ",frame:" + App.frame);
            if (data.isBuff)
            {
                List<int> curIds = this.target.mObjects;
                for (int i = 0; i < curIds.Count; i++)
                {
                    Role role = RoleManager.Get(curIds[i]);
                    AttrPropAction.RemoveBuff(role, data);
                }
            }
        }
        //public override void OnStop()
        //{
        //   // Debug.Log("OnStop:" + this.root.target.mObjects.Count + ",frame:" + App.frame);
        //    base.OnStop();
        //}
        //public override void OnDestroy()
        //{
        // //   Debug.Log("OnDestroy:" + this.root.target.mObjects.Count + ",frame:" + App.frame);
        //    base.OnDestroy();
        //}
        //public PropValue GetValue()
        //{
        //   // Debug.Log("GetValue:" + this.root.target.mObjects.Count + ",frame:" + App.frame);
        //    return this.data.value;
        //}
    }
}