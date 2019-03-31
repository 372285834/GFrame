using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    [Time("数据/动作", typeof(EffectData))]
    public class AnimatorStyle : ResStyle
    {
        public string clip;
        public bool isSelf = true;
        public float speed = 1f;
        public float duration = 0.1f;
    }
    public class AnimatorData : ResData
    {
        public Animator animator;
        public override void OnInit()
        {

        }
        public override void OnTrigger()
        {
            AnimatorStyle style = this.style as AnimatorStyle;
            if (style.isSelf)
                animator = this.owner.animator;
            else
                animator = this.root.target.getObj().animator;
            animator.speed = style.speed;
            animator.CrossFadeInFixedTime(style.clip, style.duration);
        }
        public override void OnFinish()
        {
            animator = null;
        }
    }

}
