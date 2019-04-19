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
        [Desc("目标")]
        public TargetData target;
        public Role obj
        {
            get
            {
                if (target == null)
                    return this.owner;
                return this.target.obj;
            }
        }
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            AnimatorStyle style = data.style as AnimatorStyle;
            obj.PlayClip(style.clip, style.duration, style.speed);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {

        }
    }
}