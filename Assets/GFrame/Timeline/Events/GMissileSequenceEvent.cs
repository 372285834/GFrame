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
        public int[] intervals;
        public int duration;
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
            
        }

        protected override void OnStop()
        {

        }
        protected override void OnFinish()
        {

        }
    }
}
