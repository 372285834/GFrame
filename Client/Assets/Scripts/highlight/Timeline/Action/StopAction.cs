
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/停止_root", typeof(StopAction))]
    public class StopAction : TimeAction
    {
        public override TriggerStatus OnTrigger()
        {
            this.root.Stop();
            return TriggerStatus.Failure;
        }
    }
}