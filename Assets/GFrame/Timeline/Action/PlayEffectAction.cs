using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/播放特效", typeof(PlayEffectAction))]
    public class PlayEffectAction : TimeAction
    {
        public override TriggerStatus OnTrigger()
        {
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {

        }
    }
}