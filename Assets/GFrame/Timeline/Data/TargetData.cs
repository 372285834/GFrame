using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/目标", typeof(TargetData))]
    public class TargetStyle : ComponentStyle
    {
        public int index;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {

        }
#endif
    }
    public class TargetData : ComponentData
    {
        public Role obj;
        public override bool OnTrigger()
        {
            obj = this.root.target.getObj((this.style as TargetStyle).index);
            return true;
            //this.prefabData.transform
            //this.root.target.getObj
        }
        public override void OnStop()
        {
            obj = null;
        }
    }
}