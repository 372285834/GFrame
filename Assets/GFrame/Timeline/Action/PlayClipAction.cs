using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/播放动画", typeof(PlayClipAction))]
    public class PlayClipAction : TimeAction
    {
        [Desc("动画数据")]
        public AnimatorData data;
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            AnimatorStyle style = data.style as AnimatorStyle;
            data.animator.speed = style.speed;
            data.animator.CrossFadeInFixedTime(style.clip, style.duration);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {

        }
    }
}