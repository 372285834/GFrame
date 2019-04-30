
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/角色状态", typeof(StringKeyData))]
    public class StringKeyStyle : ComponentStyle
    {
        public string key;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class StringKeyData : ComponentData
    {
        public string key;
        public override void OnInit()
        {
            key = (this.style as StringKeyStyle).key;// state;
        }
    }
}