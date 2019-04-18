
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
        public BoolValue value;
        public BoolAttrType attrType;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class AttrBoolData : ComponentData
    {
        public BoolValue value;
        public BoolAttrType attrType;
        public override void OnInit()
        {

        }
        public bool GetBool()
        {
            return true;
        }
    }
}