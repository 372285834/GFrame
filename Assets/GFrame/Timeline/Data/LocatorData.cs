
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight
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
    public class LocatorData : ComponentData
    {
        public LocatorStyle loStyle { get { return this.style as LocatorStyle; } }
        public Locator locator { get { return (this.style as LocatorStyle).locator; } }
        public Vector3 curPos
        {
            get
            {
                if(target != null && loStyle.isFollow)
                {
                    pos = target.position + loStyle.off;
                }
                 return pos;
            }
        }
        public Vector3 pos;
        public Transform target { get; private set; }

        public override TriggerStatus OnTrigger()
        {
            SceneObject targetObj = null;
            //pos = locator.position;
            //SceneObject mObj = this.prefabData.obj;
            switch (locator.type)
            {
                case Locator.eType.LT_OWNER:
                    targetObj = this.root.owner;
                    break;
                case Locator.eType.LT_TARGET:
                    targetObj = this.root.target.getObj(index);
                    break;
                case Locator.eType.LT_TARGET_POS:
                    pos = this.root.target.getPos(index);
                    break;
                case Locator.eType.LT_SCENE:
                    pos = loStyle.off;
                    break;
                case Locator.eType.LT_PARENT:
                    targetObj = this.timeObject.parent.resData.obj;
                    break;
                case Locator.eType.LT_PARENT_POS:
                    targetObj = this.timeObject.parent.resData.obj;
                    pos = targetObj.getPosition();
                    break;
                default:
                    break;
            }
            if (targetObj != null)
            {
                target = targetObj.getLocator(locator.parentName);
                pos = target.position + loStyle.off;
            }
            return TriggerStatus.Success;
            //this.prefabData.transform
            //this.root.target.getObj
        }
        public override void OnStop()
        {
            this.target = null;
            pos = Vector3.zero;
        }
    }
}