
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    [Time("数据/挂点", typeof(LocatorData))]
    public class LocatorStyle : ComponentStyle
    {
        public Locator locator;
    }
    public class LocatorData : ComponentData, IPosition
    {
        public Locator locator { get { return (this.style as LocatorStyle).locator; } }
        public Vector3 getPosition
        {
            get
            {
                if (!locator.isFollow)
                    return pos;
                return target.position + locator.position;
            }
        }
        Vector3 pos;
        Transform target = null;
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
                    targetObj = this.root.target.getObj();
                    break;
                case Locator.eType.LT_TARGET_POS:
                    pos = this.root.target.getPosition();
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