using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
    [Action("行为/播放引用timeline", typeof(PlayTimelineAction))]
    public class PlayTimelineAction : TargetAction
    {
        [Desc("TimelineStyle")]
        public StringKeyData data;
        Timeline mTimeline;
        public override TriggerStatus OnTrigger()
        {
            mTimeline = TimelineFactory.Creat(data.key,this.role);
            if(mTimeline == null)
            {
                return TriggerStatus.Failure;
            }
            mTimeline.skill = this.root.skill;
            mTimeline.Play(0);
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            if (mTimeline != null)
                mTimeline.UpdateFrame(App.deltaFrame);
        }
        public override void OnStop()
        {
            if (mTimeline != null)
                mTimeline.Stop();
            base.OnStop();
        }
        public override void OnDestroy()
        {
            if (mTimeline != null)
                mTimeline.Destroy();
            mTimeline = null;
        }
    }
}