using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using highlight.timeline;
namespace highlight
{
    public class TimelineNode : TimeNode
    {
        public TimelineStyle timelineStyle { get { return style as TimelineStyle; } }
       
        public Timeline timeline { get { return obj as Timeline; } }
        public static TimelineNode Creat(TimelineStyle _style)
        {
            GameObject go = new GameObject(_style.name);
            go.hideFlags = HideFlags.DontSave;
            TimelineNode node = go.AddComponent<TimelineNode>();
            node.obj = _style.Creat();
            node.parent = null;
            node.root = node;
            node.CreatChild(node);
            return node;
        }
        public bool isChange = false;

    }
}