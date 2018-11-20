using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    [GEvent("Trigger/PlayTimeline", typeof(GPlayTimelineEvent))]
    [Serializable]
    public class GPlayTimelineStyle : GEventStyle
    {
        public string styleRes;
    }
    public class GPlayTimelineEvent : GEvent
    {
        protected override void OnInit()
        {

        }
        protected override void OnTrigger(int framesSinceTrigger, float timeSinceTrigger)
        {
            //GTriggerStyle s = (GTriggerStyle)this.mStyle;
            //GTimeline tl = GTimelineFactory.GetTimeline(s.name);
            //tl.setParent(this.timeLine);
            //tl.setData(this.timeLine.getData());
            //tl.Play(0);
        }

        protected override void OnStop()
        {

        }
        protected override void OnDestroy()
        {

        }
    }
}