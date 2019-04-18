using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
    [Action("行为/播放引用timeline", typeof(PlayTimelineAction))]
    public class PlayTimelineAction : TimeAction
    {
        [Desc("目标挂点")]
        public ITimelineHandler target;

        public override TriggerStatus OnTrigger()
        {
            target.timeline = TimelineFactory.Creat(target.timelineStyle);
            if(target.timeline == null)
            {
                return TriggerStatus.Failure;
            }
            target.timeline.Play(this.root.timeSinceTrigger);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
        }
    }
}