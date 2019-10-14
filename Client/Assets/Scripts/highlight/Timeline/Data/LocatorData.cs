
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
        public bool isRealTime = false;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Locator l = this.locator;//, string name
            // GUILayout.Space(10f);
            this.index = EditorGUILayout.IntField("目标index:", this.index);
            l.type = (Locator.eType)EditorGUILayout.EnumPopup("类型：", l.type);
            l.eName = (Locator.eNameType)EditorGUILayout.EnumPopup("挂点名:", l.eName);
            this.off = EditorGUILayout.Vector3Field("偏移：", this.off);
            this.isRealTime = EditorGUILayout.Toggle("实时计算：", this.isRealTime);
            this.locator = l;
        }
#endif
    }
    public class LocatorData : ComponentData<LocatorStyle>, IVector3, ITransform
    {
        public Locator locator { get { return mStyle.locator; } }
        Vector3 curPos;
        public Vector3 vec3
        {
            get
            {
                if (mStyle.isRealTime)
                    OnTrigger();
                if (transform != null && mStyle.isFollow)
                {
                    curPos = transform.position + mStyle.off;
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
                    if (targetObj == null)
                        return false;
                    break;
                case Locator.eType.LT_TARGET:
                    targetObj = this.target.getObj(mStyle.index);
                    if (targetObj == null)
                        return false;
                    break;
                case Locator.eType.LT_TARGET_POS:
                    if (!this.target.checkIndex(mStyle.index))
                        return false;
                    curPos = this.target.getPos(mStyle.index);
                    break;
                case Locator.eType.LT_SCENE:
                    curPos = mStyle.off;
                    break;
                case Locator.eType.LT_PARENT:
                    targetObj = this.timeObject.parent.role;
                    if (targetObj == null)
                        return false;
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
                if(locator.eName == Locator.eNameType.ROOT )
                {
                    curPos = targetObj.position;
                   // Debug.Log(curPos);
                    transform = targetObj.transform;
                    return true;
                }
                transform = targetObj.getLocator(locator.parentName);
                if(transform == null)
                {
                    return false;
                }
                curPos = transform.position + mStyle.off;
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