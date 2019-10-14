using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
    [Action("行为/播放引用timeline", typeof(PlayTimelineAction))]
    public class PlayTimelineAction : TargetAction
    {
        [Desc("TimelineStyle")]
        public StringKeyData data;
        public Timeline mTimeline;
        public override void OnUpdate()
        {
            if (mTimeline == null)
            {
                mTimeline = TimelineFactory.Creat(data.key, this.role);
                mTimeline.skill = this.root.skill;
                mTimeline.Play(0);
            }
            else
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
            TimelineFactory.Destroy(mTimeline);
            mTimeline = null;
        }
    }
}