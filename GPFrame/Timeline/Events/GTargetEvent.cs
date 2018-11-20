using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    //[GEvent("Skill/轨道目标", typeof(GTargetEvent))]
    public class GTargetStyle : GPlayTimelineStyle
    {
        public Locator startLocator;
        public string res;
        public Locator endLocator;
        public MTweenEase.EaseType easeType = MTweenEase.EaseType.None;
        public int curveIndex;
    }
    //public class GTargetEvent : GEvent
    //{
    //    protected override void OnInit()
    //    {

    //    }
    //}
}