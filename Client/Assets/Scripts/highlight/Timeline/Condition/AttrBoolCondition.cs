using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
namespace highlight.tl
{
    [Time("条件/Bool属性判断", typeof(AttrBoolCondition))]
    public class AttrBoolConditionStyle : ConditionStyle
    {
        public AttrType attrType;
        public bool value;
        public bool isObs;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.attrType = (AttrType)EditorGUILayout.EnumPopup("类型：", this.attrType);
             this.value = EditorGUILayout.Toggle("value：", this.value);
            //this.isObs = EditorGUILayout.Toggle("注册事件：", this.isObs);
        }
#endif
    }
    public class AttrBoolCondition : ConditionData
    {
        public AttrType attrType { get { return this.GetStyle<AttrBoolConditionStyle>().attrType; } }
        public bool value { get { return this.GetStyle<AttrBoolConditionStyle>().value; } }
        //public bool isObs { get { return this.GetStyle<AttrBoolConditionStyle>().isObs; } }
        public override void OnRegister()
        {
                this.owner.attrs.AddObs(attrType, OnAttrChange);
        }
        public override bool OnCheck()
        {
            bool v = false;
            if (attrType > AttrType.Extend_Attr)
            {
                v = GetExtendValue(attrType);
            }
            else
            {
                v = this.owner.attrs.GetBoolV(attrType, false);
            }
            if (v != value)
                return false;
            return true;
        }
        public bool GetExtendValue(AttrType t)
        {
            if (t == AttrType.have_target)
            {
                Role r = this.target.getObj(0);
                return  r!= null && !r.isClear;
            }
            if(t == AttrType.target_in_rang)
            {
                Role target = this.target.getObj(0);
                if(target != null)
                {
                    float toDis = Vector3.Distance(target.position, this.owner.position);
                    float atkRang = this.owner.attrs.GetFloat(AttrType.atk_rang);
                    if (toDis < atkRang)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        void OnAttrChange(RoleAttrs attrs)
        {
            this.OnChange();
        }
        public override void OnRemove()
        {
            this.owner.attrs.RemoveObs(attrType, OnAttrChange);
        }
    }
}