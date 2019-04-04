using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    [Action("行为/设置父节点", typeof(SetParentAction))]
    public class SetParentAction : TimeAction
    {
        [Desc("目标挂点")]
        public LocatorData target;

        public override void OnTrigger()
        {
            this.res.obj.SetParent(this.target.target);
            this.res.obj.SetLocalPos(this.target.loStyle.off);
        }
        public override void OnUpdate()
        {
        }
    }
}