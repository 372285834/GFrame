using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/属性/Int", typeof(CountData))]
    public class CountStyle : ComponentStyle
    {
        public int count;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.count = EditorGUILayout.IntField("int:", this.count);
        }
#endif
    }
    public class CountData : ComponentData<CountStyle>
    {
        public int value { get { return mStyle.count; } }
       // public int cur;
     //   public override void OnInit()
     //   {
          //  cur = count;
    //    }
    }
}