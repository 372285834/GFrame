using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.timeline
{
    [Time("数据/动画", typeof(AnimatorData))]
    public class AnimatorStyle : ComponentStyle
    {
        public string clip;
        public bool isSelf = true;
        public float speed = 1f;
        public float duration = 0.1f;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.clip = EditorGUILayout.TextField("动画名:", this.clip);
            this.isSelf = EditorGUILayout.Toggle("是否自己：", this.isSelf);
            this.speed = EditorGUILayout.FloatField("速度：", this.speed);
            this.duration = EditorGUILayout.FloatField("过渡：", this.duration);
        }
#endif
    }
    public class AnimatorData : ComponentData
    {
        public Animator animator;
        public override void OnInit()
        {

        }
        public override TriggerStatus OnTrigger()
        {
            AnimatorStyle style = this.style as AnimatorStyle;
            if (string.IsNullOrEmpty(style.clip))
            {
                return TriggerStatus.Failure;
            }
            if (style.isSelf)
                animator = this.owner.animator;
            else
                animator = this.root.target.getObj().animator;
            return animator == null ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnStop()
        {
            animator = null;
        }
    }

}
