﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/动画", typeof(AnimatorData))]
    public class AnimatorStyle : ComponentStyle
    {
        public string clip;
        public float speed = 1f;
        public float duration = 0.1f;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.clip = EditorGUILayout.TextField("动画名:", this.clip);
            this.speed = EditorGUILayout.FloatField("速度：", this.speed);
            this.duration = EditorGUILayout.FloatField("过渡：", this.duration);
        }
#endif
    }
    public class AnimatorData : ComponentData
    {

    }

}