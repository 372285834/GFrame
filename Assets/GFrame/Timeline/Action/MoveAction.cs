using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    [Action("行为/移动", typeof(MoveAction))]
    public class MoveAction : TimeAction
    {
        [Desc("开始挂点")]
        public LocatorData start;
        [Desc("结束挂点")]
        public LocatorData end;
        [Desc("运动轨迹")]
        public TrailData eva;

        public override void OnUpdate()
        {
            Vector3 pos = eva.Evaluate(this.start.curPos, this.end.curPos, this.timeObject.progress);
            this.res.obj.SetPos(pos);
        }
    }
}