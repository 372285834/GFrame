using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    public enum eEffectSequenceType
    {
        Order = 0,
        Every=1,
        Link = 2,
    }
    [GEvent("Skill/特效序列", typeof(GEffectSequenceEvent))]
    public class GEffectSequenceStyle : GEffectStyle
    {
        public eEffectSequenceType eType = eEffectSequenceType.Order;
        public int[] intervals;
    }
    public class GEffectSequenceEvent : GEvent
    {
        protected override void OnInit()
        {

        }
        protected override void OnTrigger(int framesSinceTrigger, float timeSinceTrigger)
        {
            GEffectStyle s = (GEffectStyle)this.mStyle;
            //GTimelineData lData = this.mTimelineData;
            //playeffect(s.name,s.getTargetPos());
            Locator mLocator = s.locator;
            switch (mLocator.type)
            {
                case Locator.eType.LT_MYSELF:

                    break;
                case Locator.eType.LT_TARGET:

                    break;
                case Locator.eType.LT_SCENE:

                    break;
                case Locator.eType.LT_UI:
                    break;
            }
        }

        protected override void OnStop()
        {

        }
        protected override void OnFinish()
        {

        }
    }
}
