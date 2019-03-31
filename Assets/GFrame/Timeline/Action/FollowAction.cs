using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    [Time("行为/跟随", typeof(FollowAction))]
    public class FollowStyle : ActionStyle
    {
    }
    public class FollowAction : TimeAction
    {
        public IPosition target;

        public override void OnInit()
        {
            this.target = this.GetComponent(1) as IPosition;
        }
        public override void OnUpdate()
        {
            this.prefabData.SetPos(this.target.getPosition);
        }
        public override void OnDestroy()
        {
            this.target = null;
        }
    }
}