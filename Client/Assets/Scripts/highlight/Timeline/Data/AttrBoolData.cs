
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/条件属性", typeof(AttrBoolData))]
    public class AttrBoolStyle : ComponentStyle
    {
        public bool value;
        public int level;
        public AttrType attrType;
        public int time;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.attrType = (AttrType)EditorGUILayout.EnumPopup("类型：", this.attrType);
            this.value = EditorGUILayout.Toggle("value：", this.value);
            this.level = EditorGUILayout.IntField("level：", this.level);
            this.time = EditorGUILayout.IntField("time：", this.time);
        }
#endif
    }
    public class AttrBoolData : ComponentData<AttrBoolStyle>
    {
        public BoolValue value;
        public AttrType attrType { get { return mStyle.attrType; } }
        public override bool OnTrigger()
        {
            value = new BoolValue(mStyle.value, mStyle.level);
            if (mStyle.time > 0)
                value.cd = new CDData(mStyle.time);
            return true;
        }
    }
}