using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    public enum eMissileSequenceType
    {
        Order=0,
        Every = 1,
        Link=2,
        PingPong=3,
    }
    [GEvent("Skill/导弹序列", typeof(GMissileSequenceEvent))]
    public class GMissileSequenceStyle : GMissileStyle
    {
        public eMissileSequenceType eType = eMissileSequenceType.Order;
        public float[] intervals;
    }
    public class GMissileSequenceEvent : GEvent
    {
        protected override void OnInit()
        {

        }
        protected override void OnTrigger(int framesSinceTrigger, float timeSinceTrigger)
        {
            GMissileSequenceStyle style = (GMissileSequenceStyle)this.mStyle;
            Locator mLocator = style.startLocator;
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
