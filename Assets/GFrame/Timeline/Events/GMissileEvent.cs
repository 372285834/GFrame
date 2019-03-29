using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    [GEvent("Skill/导弹", typeof(GMissileEvent))]
    public class GMissileStyle : GTargetStyle
    {
        public string res;
    }
    public class GMissileEvent : GEvent
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
