using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/播放动画", typeof(PlayClipAction))]
    public class PlayClipAction : TargetAction
    {
        [Desc("动画数据")]
        public AnimatorData data;
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            AnimatorStyle style = data.style as AnimatorStyle;
            role.PlayClip(style.clip, style.loop,style.speed);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            AnimatorStyle style = data.style as AnimatorStyle;
            if(style.loop)
                role.PlayClip(style.clip, style.loop, style.speed);
        }
    }
}