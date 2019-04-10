using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.timeline
{
    [Action("行为/运行buff", typeof(BuffAction))]
    public class BuffAction : TimeAction
    {
        [Desc("buff数据")]
        public BuffData data;
        public override TriggerStatus OnTrigger()
        {
            return data == null ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {

        }
    }
}