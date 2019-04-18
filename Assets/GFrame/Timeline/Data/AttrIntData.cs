
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/数值属性", typeof(AttrIntData))]
    public class AttrIntStyle : ComponentStyle
    {
        public IntValue value;
        public IntAttrType attrType;
        public bool once;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class AttrIntData : ComponentData
    {
        public IntValue value;
        public IntAttrType attrType;
        public bool once;
        public override void OnInit()
        {

        }
    }
}