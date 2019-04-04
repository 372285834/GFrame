using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    [Action("行为/设置坐标", typeof(SetPosAction))]
    public class SetPosAction : TimeAction
    {
        [Desc("目标挂点")]
        public LocatorData target;

        public override void OnTrigger()
        {

        }
        public override void OnUpdate()
        {
            this.res.obj.SetPos(this.target.curPos);
        }
    }
}