using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/Int", typeof(CountData))]
    public class CountStyle : ComponentStyle
    {
        public int count;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {

        }
#endif
    }
    public class CountData : ComponentData
    {
        public int count;
        public int cur;
        public override void OnInit()
        {
            count = (this.style as CountStyle).count;
        }
        public override bool OnTrigger()
        {
            cur = 0;
            return true;
        }
    }
}