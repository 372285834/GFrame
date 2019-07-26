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
        Npc_AI_1,
    }
    public enum Npc_AI_1
    {
        巡逻=0,
        战斗,
        逃跑,//
        回家,
    }
    [Time("数据/状态", typeof(StateData))]
    public class StateStyle : ComponentStyle
    {
        public StateType type;
        public int value;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            //eStateType ct;
            //Enum.TryParse<eStateType>(this.type, out ct);
            this.type = (StateType)EditorGUILayout.EnumPopup("StateType：", type);
            switch (type)
            {
                case StateType.RoleState:
                    this.value = (int)drawValue<RoleState>((RoleState)this.value);
                    //value = (int)(RoleState)EditorGUILayout.EnumPopup("RoleState：", (RoleState)this.value);
                    break;
                case StateType.Npc_AI_1:
                    this.value = (int)drawValue<Npc_AI_1>((Npc_AI_1)this.value);
                   // value = (int)(Npc_AI_1)EditorGUILayout.EnumPopup("Npc_AI_1：", (Npc_AI_1)this.value);
                    break;
                default:
                    break;
            }
        }
        public T drawValue<T>(Enum t) where T : Enum
        {
            System.Type key = typeof(T);
            return (T) EditorGUILayout.EnumPopup(key.Name, t);
        }
#endif
    }
    public class StateData : ComponentData
    {
        public int value { get { return this.GetStyle<StateStyle>().value; } }
        public StateType type { get { return GetStyle<StateStyle>().type; } }
    }
}