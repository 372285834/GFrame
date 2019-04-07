﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using highlight.timeline;
using System;

namespace highlight
{
    public class TimeNode : MonoBehaviour
    {
        public TimeStyle style { get { return obj == null ? null : obj.timeStyle; } }
        public TimeObject obj;
        public int index;
        public TimelineNode root;
        public TimeNode parent;
        public int Depth
        {
            get
            {
                int dep = 0;
                var _parent = this.parent;
                while (_parent != null)
                {
                    dep++;
                    _parent = _parent.parent;
                }
                return dep;
            }
        }
        public bool isRoot { get { return this is TimelineNode; } }
        public void CreatChild(TimelineNode root)
        {
            if (obj.Childs.Count == 0)
            {
                UnityEditor.Selection.activeObject = this;
                return;
            }
            for (int i = 0; i < obj.Childs.Count; i++)
            {
                TimeNode node = creatNode(obj.Childs[i], root);
                node.CreatChild(root);
            }
        }
        TimeNode creatNode(TimeObject _obj, TimelineNode root)
        {
            GameObject go = new GameObject(_obj.timeStyle.name);
            TimeNode node = go.AddComponent<TimeNode>();
            node.index = _obj.index;
            node.transform.SetParent(this.transform);
            node.obj = _obj;
            node.parent = this;
            node.root = root;
            return node;
        }
        public virtual void UpdateData()
        {
            if (this.style == null)
                return;
            this.style.name = this.name;
        }

        public TimeNode AddChild(TimeStyle style)
        {
            if (style == null)
            {
                style = new TimeStyle();
                style.Range = this.style.Range;
                style.name = "node" + this.root.timeline.AllCount;
            }
            TimeObject _obj = this.obj.AddChild(style);
            TimeNode node = creatNode(_obj, root);
            node.CreatChild(root);
            return node;
        }
        public void RemoveChild(TimeNode node)
        {
            this.obj.RemoveChild(node.obj);
            GameObject.DestroyImmediate(node.gameObject);
        }
        public void SetChildIndex(TimeNode node, int idx)
        {
            this.obj.SetChildIndex(node.obj, idx);
        }
        public void AddComponent(ComponentStyle t)
        {
            this.obj.AddComponent(t);
        }
        public void AddAction(ActionStyle t)
        {
            this.obj.AddAction(t);
        }
        public void RemoveComponent(ComponentData t)
        {
            this.obj.RemoveComponent(t);
        }
        public void SetComponentIndex(ComponentData t, int idx)
        {
            this.obj.SetComponentIndex(t, idx);
        }
        public void RemoveAction(TimeAction t)
        {
            this.obj.RemoveAction(t);
        }
        public void SetActionIndex(TimeAction t, int idx)
        {
            this.obj.SetActionIndex(t, idx);
        }
    }
}