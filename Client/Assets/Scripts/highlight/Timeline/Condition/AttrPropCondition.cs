
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace highlight.tl
{
    [Time("条件/Int属性判断", typeof(AttrPropCondition))]
    public class AttrPropConditionStyle : IntConditionStyle
    {
        public AttrType attrType;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.attrType = (AttrType)EditorGUILayout.EnumPopup("类型：", this.attrType);
            base.OnInspectorGUI();
        }
#endif
    }
    public class AttrPropCondition : ComponentData<AttrPropConditionStyle>, IConditionData
    {
        public LogicType logicType { get { return mStyle.logicType; } }
        public int value { get { return mStyle.value; } }
        public AttrCompareType cType { get { return mStyle.cType; } }
        public bool isObs { get { return mStyle.isObs; } }
        public AttrType attrType { get { return mStyle.attrType; } }

        public bool OnCheck()
        {
            int v = 0;
            if(attrType > AttrType.Extend_Attr)
            {
                v = GetExtendValue(attrType);
            }
            else
                v = this.owner.attrs.GetInt(attrType, false);
            return IntConditionStyle.CompareValue(v, this.cType, this.value);
        }

        public int GetExtendValue(AttrType t)
        {
            int v = 0;
            if (t == AttrType.hp_percent)
            {
                v = this.owner.attrs.hp_percent;
            }
            else if (t == AttrType.target_dis)
            {
                v = this.target.GetDis(this.owner.position);
            }
            else if(t == AttrType.target_dis_pos)
            {
                v = this.target.GetDisPos(this.owner.position);
            }
            else if (t == AttrType.source_dis && this.owner.npcMapData != null)
            {
                float dis = Vector3.Distance(this.owner.position, this.owner.npcMapData.Pos);
                v = Mathf.RoundToInt(dis * 1000);
            }
            return v;
        }

        //AcHandler OnChangeFunc;
        //public void OnRegister(AcHandler _ac)
        //{
        //    OnChangeFunc = _ac;
        //    this.owner.attrs.AddObs(attrType, OnAttrChange);
        //}
        //void OnAttrChange(RoleAttrs attrs)
        //{
        //    if (OnChangeFunc != null)
        //        OnChangeFunc();
        //}
        //public void OnRemove()
        //{
        //    if (OnChangeFunc != null)
        //    {
        //        this.owner.attrs.RemoveObs(attrType, OnAttrChange);
        //        OnChangeFunc = null;
        //    }
        //}
    }
}