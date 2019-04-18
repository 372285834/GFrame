using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    public class TimelineNode : TimeNode
    {
        public TimelineStyle timelineStyle { get { return style as TimelineStyle; } }
       
        public Timeline timeline { get { return obj as Timeline; } }
        public static TimelineNode Creat(TimelineStyle _style)
        {
            GameObject go = new GameObject(_style.name);
            //go.hideFlags = HideFlags.DontSave;
            TimelineNode node = go.AddComponent<TimelineNode>();
            Timeline tl = _style.Creat();
            tl.DestroyOnStop = false;
            node.obj = tl;
            node.parent = null;
            node.root = node;
            node.CreatChild(node);
            return node;
        }
        public bool isChange = false;
    }
}