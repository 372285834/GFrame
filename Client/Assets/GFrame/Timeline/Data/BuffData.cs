using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/buff", typeof(BuffData))]
    public class BuffStyle : ComponentStyle
    {
        public int id;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
           // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
           // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class BuffData : ComponentData
    {
        public int id;
        public override void OnInit()
        {

        }
    }
}