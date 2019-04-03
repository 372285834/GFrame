
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
        public int index;
        public Locator locator;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            Locator l = this.locator;//, string name
            // GUILayout.Space(10f);
            this.index = EditorGUILayout.IntField("目标index:", this.index);
            l.isFollow = EditorGUILayout.Toggle("是否跟随：", l.isFollow);
            l.type = (Locator.eType)EditorGUILayout.EnumPopup("类型：", l.type);
            l.eName = (Locator.eNameType)EditorGUILayout.EnumPopup("挂点名:", l.eName);
            l.position = EditorGUILayout.Vector3Field("偏移：", l.position);
            this.locator = l;
        }
#endif
    }
    public class LocatorData : ComponentData
    {
        public Locator locator { get { return (this.style as LocatorStyle).locator; } }
        public Vector3 position
        {
            get
            {
                if (!locator.isFollow)
                    return pos;
                return target.position + locator.position;
            }
        }
        Vector3 pos;
        public Transform target { get; private set; }
        public override void OnInit()
        {
            
        }
        public override void OnTrigger()
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
                    pos = locator.position;
                    break;
                case Locator.eType.LT_PARENT:
                    PrefabData pre = this.timeObject.parent.resData as PrefabData;
                    targetObj = pre.obj;
                    break;
                case Locator.eType.LT_PARENT_POS:
                    PrefabData pre2 = this.timeObject.parent.resData as PrefabData;
                    pos = pre2.obj.getPosition();
                    break;
                default:
                    break;
            }
            if (targetObj != null)
            {
                target = targetObj.getLocator(locator.parentName);
                pos = target.position + locator.position;
            }
            //this.prefabData.transform
            //this.root.target.getObj
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            this.target = null;
            pos = Vector3.zero;
        }
    }
}