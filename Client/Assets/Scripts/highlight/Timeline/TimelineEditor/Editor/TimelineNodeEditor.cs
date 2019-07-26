
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace highlight.tl
{
    [CanEditMultipleObjects, CustomEditor(typeof(TimelineNode))]
    [ExecuteInEditMode]
    public class TimelineNodeEditor : TimeNodeEditor
    {
        TimelineNode mTimelineNode;
        TimelineStyle _style;
        Timeline _timeline;
        //public string[] stringKeys = new string[] {
        //    "AIState",
        //};
        //class kvData
        //{
        //    public string k;
        //    public int v;
        //}
        //List<kvData> list;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //mTimelineNode = target as TimelineNode;
            //_style = mTimelineNode.timelineStyle;
            //if (_style == null)
            //    return;
            //_timeline = mTimelineNode.timeline;
            /*
            list = new List<kvData>();
            string[] ks = style.keys;
            if(ks != null && ks.Length > 0)
            {
                for (int i = 0; i < style.keys.Length; i++)
                {
                    list.Add(new kvData() { k = style.keys[i], v = style.values[i] });
                }
            }
            else
            {
                if (GUILayout.Button("+"))
                {
                    list.Add(new kvData());
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    list.Insert(i, new kvData());
                    ResetData();
                    return;
                }
                if (GUILayout.Button("-"))
                {
                    list.RemoveAt(i);
                    ResetData();
                    return;
                }
                GUILayout.EndHorizontal();
                int idx = 0;
                for (int j = 0; j < stringKeys.Length; j++)
                {
                    if (stringKeys[j] == list[i].k)
                    {
                        idx = j;
                        break;
                    }
                }
                //idx = EditorGUILayout.Popup("key", idx, stringKeys);

                //list[i].k = stringKeys[idx];
                list[i].k = EditorGUILayout.TextField("key", list[i].k);
                list[i].v = EditorGUILayout.IntField("value", list[i].v);

                GUILayout.Space(10f);
            }
            // ResetData();
            */
            //GUILayout.BeginVertical();
            //this.Init();
            //ShowMenu();
            ////drawRang();
            //DrawComponents();
            //GUILayout.Space(10f);
            //GUILayout.EndVertical();
        }
        //void ResetData()
        //{
        //    style.keys = new string[list.Count];
        //    style.values = new int[list.Count];
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        style.keys[i] = list[i].k;
        //        style.values[i] = list[i].v;
        //    }
        //}
    }
}