using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/轨迹", typeof(TrailData))]
    public class TrailStyle : ComponentStyle
    {
        public MTweenEase.EaseType type = MTweenEase.EaseType.None;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.type = (MTweenEase.EaseType)EditorGUILayout.EnumPopup("速度类型:", this.type);
        }
#endif
    }
    public class TrailData : ComponentData, IEvaluateV3
    {
        public Vector3 Evaluate(Vector3 start, Vector3 end, float time)
        {
            MTweenEase.EaseType type = (this.style as TrailStyle).type;
            Vector3 pos = Vector3.zero;
            float p = MTweenEase.ease(type, 0f, 1f, time);
            //      pos = Vector3.Lerp(start, end, p);
            //  else
            //     Vector3 dir = end - start;
            return Vector3.Lerp(start, end, p);
        }
    }

}