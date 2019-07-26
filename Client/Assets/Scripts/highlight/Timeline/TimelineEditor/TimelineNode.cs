using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace highlight.tl
{
    public class TimelineNode : TimeNode
    {
        public static Timeline Current;
        public static TimelineNode CurRoot;
        public TimelineStyle timelineStyle { get { return style as TimelineStyle; } }
       
        public Timeline timeline { get { return obj as Timeline; } }
        public static TimelineNode Creat(TimelineStyle _style)
        {
            Timeline tl = _style.Creat();
            return Creat(tl);
        }
        public static TimelineNode Creat(Timeline tl)
        {
            Debug.Log("CreatTimeline:" + tl.name);
            GameObject go = new GameObject(tl.name);
            //go.hideFlags = HideFlags.DontSave;
            go.tag = "Timeline";
            TimelineNode node = go.AddComponent<TimelineNode>();
            tl.DestroyOnStop = false;
            node.obj = tl;
            node.parent = null;
            node.root = node;
            node.CreatChild(node);
            Current = tl;
            CurRoot = node;
            return node;
        }
        public bool isChange = false;
#if UNITY_EDITOR
        public static List<string> keyList = new List<string>();
        public static string DrawGlobal(string k)
        {
            if(Selection.activeGameObject == CurRoot)
            {
                return EditorGUILayout.TextField("key", k);
            }
            List<ComponentData> keys = TimelineNode.Current.ComponentList;
            if (keys == null || keys.Count == 0)
                return "";
            int idx = 0;
            keyList.Clear();
            for (int i = 0; i < keys.Count; i++)
            {
                GlobalData data = keys[i] as GlobalData;
                    if (data.key == k)
                    {
                        idx = i;
                        break;
                    }
                    keyList.Add(data.key);
            }
            if(keyList.Count > 0)
            {
                idx = EditorGUILayout.Popup("global_key", idx, keyList.ToArray());
                return keyList[idx];
            }
            return "";
        }
#endif
    }

}