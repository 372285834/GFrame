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

        public override void OnInit()
        {

        }
        public override void OnTrigger()
        {
            this.prefabData.SetParent(this.target.target);
        }
        public override void OnUpdate()
        {
        }
        public override void OnDestroy()
        {
            this.target = null;
        }
    }
}