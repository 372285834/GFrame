
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/属性/String", typeof(StringKeyData))]
    public class StringKeyStyle : ComponentStyle
    {
        public string key;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
             this.key = EditorGUILayout.TextField("key：", this.key);
        }
#endif
    }
    public class StringKeyData : ComponentData<StringKeyStyle>
    {
        public string key { get { return mStyle.key; } }
    }
}