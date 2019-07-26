using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/属性/Vector3", typeof(Vector3Data))]
    public class Vector3Style : ComponentStyle
    {
        public Vector3 vec3;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class Vector3Data : ComponentData, IVector3
    {
        public Vector3 vec3 { get; set; }
        public override void OnInit()
        {
            vec3 = (this.style as Vector3Style).vec3;// state;
        }
    }
}