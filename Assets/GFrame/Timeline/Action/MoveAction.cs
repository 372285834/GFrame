using highlight.timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    [Time("行为/移动", typeof(MoveAction))]
    public class MoveStyle : ActionStyle
    {
        public MTweenEase.EaseType type = MTweenEase.EaseType.None;
    }
    public class MoveAction : TimeAction
    {
        public IPosition start;
        public IPosition end;
        public IEvaluate eva;

        public override void OnInit()
        {
            this.start = this.GetComponent(0) as IPosition;
            this.end = this.GetComponent(1) as IPosition;
            this.eva = this.GetComponent(2) as IEvaluate;
        }
        public override void OnUpdate()
        {
            Vector3 pos = Vector3.zero;
            float p = MTweenEase.ease((this.style as MoveStyle).type, 0f, 1f, this.timeObject.progress);
            if(eva == null)
                pos = Vector3.Lerp(this.start.getPosition, this.end.getPosition, p);
            else
                pos = eva.Evaluate(this.start.getPosition, this.end.getPosition, p);
            this.prefabData.SetPos(pos);
        }
        public override void OnDestroy()
        {
            this.start = null;
            this.end = null;
            this.eva = null;
        }
    }
}