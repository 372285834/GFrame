using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    [Action("行为/设置坐标", typeof(SetPosAction))]
    public class SetPosAction : TimeAction
    {
        [Desc("目标挂点")]
        public IPosition target;

        public override bool OnTrigger()
        {
            return true;
        }
        public override void OnUpdate()
        {
            this.res.obj.SetPos(this.target.pos);
        }
    }
}