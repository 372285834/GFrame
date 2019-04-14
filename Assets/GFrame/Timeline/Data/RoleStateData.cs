using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.timeline
{
    [Time("数据/角色状态", typeof(RoleStateData))]
    public class RoleStateStyle : ComponentStyle
    {
        public string state;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
           // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
           // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class RoleStateData : ComponentData
    {
        public string curState;
        public override void OnInit()
        {
            curState = (this.style as RoleStateStyle).state;// state;
        }
    }
}