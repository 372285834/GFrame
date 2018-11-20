using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    [GEvent("Skill/特效", typeof(GEffectEvent))]
    public class GEffectStyle : GEventStyle
    {
        public Locator locator;
        public string effRes;
    }
    public class GEffectEvent : GEvent
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
        protected override void OnDestroy()
        {

        }
    }
}
