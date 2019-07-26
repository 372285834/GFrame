
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/数值属性", typeof(AttrPropData))]
    public class AttrPropStyle : ComponentStyle
    {
        public int value;
        public AttrType attrType;
      //  public int count;
        public bool isBuff;
        public int time;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.isBuff = EditorGUILayout.Toggle("isBuff：", this.isBuff);
            this.attrType = (AttrType)EditorGUILayout.EnumPopup("类型：", this.attrType);
            this.value = EditorGUILayout.IntField("value：", this.value);
            this.time = EditorGUILayout.IntField("length：", this.time);
            //    this.count = EditorGUILayout.IntField("count：", this.count);
        }
#endif
    }
    [Time("数据/数值属性_完整", typeof(AttrPropData))]
    public class AttrPropAllStyle : AttrPropStyle
    {
        public int extraValue;
        public int basePer;
        public int totalPer;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.extraValue = EditorGUILayout.IntField("extraValue：", this.extraValue);
            this.basePer = EditorGUILayout.IntField("basePer：", this.basePer);
            this.totalPer = EditorGUILayout.IntField("totalPer：", this.totalPer);
        }
#endif
    }
    public class AttrPropData : ComponentData
    {
        public PropValue value;
        public AttrType attrType { get { return GetStyle<AttrPropStyle>().attrType; } }
       // public int count { get { return GetStyle<AttrPropStyle>().count; } }
        public bool isBuff { get { return GetStyle<AttrPropStyle>().isBuff; } }
        public override bool OnTrigger()
        {
            AttrPropStyle s = GetStyle<AttrPropStyle>();
            if (s is AttrPropAllStyle)
            {
                AttrPropAllStyle sa = s as AttrPropAllStyle;
                value = new PropValue(sa.value, sa.extraValue, sa.basePer, sa.totalPer);
            }
            else
            {
                value = new PropValue(s.value);
            }
            if (s.time > 0)
                value.cd = new CDData(s.time);
            return true;
        }
    }
}