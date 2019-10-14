using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    public class GlobalStyle : ComponentStyle
    {
        public string name;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.name = EditorGUILayout.TextField("key", name);
        }
#endif
    }
    [Time("全局/全局Int", typeof(GlobalIntData))]
    public class GlobalIntStyle: GlobalStyle
    {
        public int value;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.value = EditorGUILayout.IntField("value：", this.value);
        }
#endif
    }
    public class GlobalIntData : ComponentData<GlobalIntStyle>, IGlobalData//, IStateValue
    {
        public int value { get { return mStyle.value; } }
        public string key { get { return mStyle.name; } }
        public virtual void Switch()
        {
            this.root.SetGlobalValue(this.key, this.value);
        }
        public virtual object GetValue()
        {
            return value;
        }
        public virtual int GetCurValue()
        {
            return this.root.GetGlobal<int>(key);
        }
    }


    [Time("全局/全局String", typeof(GlobalStringData))]
    public class GlobalStringStyle : GlobalStyle
    {
        public string value;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.value = EditorGUILayout.TextField("value：", this.value);
        }
#endif
    }
    public class GlobalStringData : ComponentData<GlobalStringStyle>, IGlobalData
    {
        public string value { get { return mStyle.value; } }
        public string key { get { return mStyle.name; } }
        public virtual void Switch()
        {
            this.root.SetGlobalValue(this.key, this.value);
        }
        public virtual object GetValue()
        {
            return value;
        }
        public virtual string GetCurValue()
        {
            return this.root.GetGlobal<string>(key);
        }
    }
}