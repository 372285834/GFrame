
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
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
    public class AttrPropCondition : IntCondition
    {
        public AttrType attrType { get { return GetStyle<AttrPropConditionStyle>().attrType; } }
        public override void OnRegister()
        {
                this.owner.attrs.AddObs(attrType, OnAttrChange);
        }
        void OnAttrChange(RoleAttrs attrs)
        {
            OnChange();
        }
        public override bool OnCheck()
        {
            int v = 0;
            if(attrType > AttrType.Extend_Attr)
            {
                v = GetExtendValue(attrType);
            }
            else
                v = this.owner.attrs.GetInt(attrType, false);
            return CompareValue(v, this.cType, this.value);
        }
        public override void OnRemove()
        {
                this.owner.attrs.RemoveObs(attrType, OnAttrChange);
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
    }


    [Time("条件/全局Int判断", typeof(GlobalIntCondition))]
    public class GlobalIntConditionStyle : IntConditionStyle
    {
        public string global_key;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.global_key = EditorGUILayout.TextField("key", global_key);
            base.OnInspectorGUI();
        }
#endif
    }
    public class GlobalIntCondition : IntCondition
    {
        public string global_key { get { return (this.style as GlobalIntConditionStyle).global_key; } }
        public override void OnRegister()
        {
                this.root.globalObs.AddObserver(OnAttrChange);
        }
        void OnAttrChange(string k, object v)
        {
            this.OnChange();
        }
        public override bool OnCheck()
        {
            int v = this.root.GetGlobal<int>(this.global_key);
            return CompareValue(v, this.cType, this.value);
        }
        public override void OnRemove()
        {
            this.root.globalObs.RemoveObserver(OnAttrChange);
        }
    }
}