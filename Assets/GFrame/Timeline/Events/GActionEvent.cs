using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    [GEvent("Skill/动作",typeof(GActionEvent))]
    [Serializable]
    public class GActionStyle : GEventStyle
    {
        public bool isSelf = true;
        public string res;
        public bool isLoop = false;
        public int loopCount = 0;
    }
    public class GActionEvent : GEvent
    {
        protected override void OnInit()
        {

        }
        protected override void OnTrigger(int framesSinceTrigger, float timeSinceTrigger)
        {
            GActionStyle s = (GActionStyle)this.mStyle;
            if(s.isSelf)
            {

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
