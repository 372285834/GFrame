using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    public enum StateType
    {
        RoleState,
    }
    [Time("数据/状态", typeof(StateData))]
    public class StateStyle : ComponentStyle
    {
        public StateType type;
        public int state;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
           // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
           // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class StateData : ComponentData
    {
        private int _curState;
        public int curState
        {
            get
            {
                return _curState;
            }
            set
            {
                _curState = value;
            }
        }
        public StateType stateType { get { return (this.style as StateStyle).type; } }
        public override void OnInit()
        {
            _curState = (this.style as StateStyle).state;// state;
        }
    }
}