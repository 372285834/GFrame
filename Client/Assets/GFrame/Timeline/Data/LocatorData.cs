﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/挂点", typeof(LocatorData))]
    public class LocatorStyle : ComponentStyle
    {
        public int index = 0;
        public Locator locator;
        public bool isFollow = false;
        public Vector3 off;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            Locator l = this.locator;//, string name
            // GUILayout.Space(10f);
            this.index = EditorGUILayout.IntField("目标index:", this.index);
            l.type = (Locator.eType)EditorGUILayout.EnumPopup("类型：", l.type);
            l.eName = (Locator.eNameType)EditorGUILayout.EnumPopup("挂点名:", l.eName);
            this.off = EditorGUILayout.Vector3Field("偏移：", this.off);
            this.locator = l;
        }
#endif
    }
    public class LocatorData : ComponentData, IPosition,ITransform
    {
        public LocatorStyle loStyle { get { return this.style as LocatorStyle; } }
        public Locator locator { get { return (this.style as LocatorStyle).locator; } }
        Vector3 curPos;
        public Vector3 pos
        {
            get
            {
                if (transform != null && loStyle.isFollow)
                {
                    curPos = transform.position + loStyle.off;
                }
                return curPos;
            }
            set
            {
                curPos = value;
            }
        }
        public Transform transform { get; set; }

        public override bool OnTrigger()
        {
            Role targetObj = null;
            //pos = locator.position;
            //SceneObject mObj = this.prefabData.obj;
            switch (locator.type)
            {
                case Locator.eType.LT_OWNER:
                    targetObj = this.owner;
                    break;
                case Locator.eType.LT_TARGET:
                    targetObj = this.root.target.getObj(index);
                    break;
                case Locator.eType.LT_TARGET_POS:
                    if (!this.root.target.checkIndex(index))
                        return false;
                    curPos = this.root.target.getPos(index);
                    break;
                case Locator.eType.LT_SCENE:
                    curPos = loStyle.off;
                    break;
                case Locator.eType.LT_PARENT:
                    targetObj = this.timeObject.parent.role;
                    break;
                case Locator.eType.LT_PARENT_POS:
                    targetObj = this.timeObject.parent.role;
                    if (targetObj == null || targetObj.isClear)
                    {
                        return false;
                    }
                    curPos = targetObj.position;
                    break;
                default:
                    break;
            }
            if (targetObj != null)
            {
                if (targetObj.isClear)
                {
                    return false;
                }
                transform = targetObj.getLocator(locator.parentName);
                if(transform == null)
                {
                    return false;
                }
                curPos = transform.position + loStyle.off;
            }
            return true;
            //this.prefabData.transform
            //this.root.target.getObj
        }
        public override void OnStop()
        {
            this.transform = null;
            curPos = Vector3.zero;
        }
    }
}