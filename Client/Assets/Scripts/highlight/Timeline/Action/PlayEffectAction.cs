using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/播放特效", typeof(PlayEffectAction))]
    public class PlayEffectAction : TimeAction
    {
        public override bool OnTrigger()
        {
            return true;
        }
        public override void OnUpdate()
        {

        }
    }
}