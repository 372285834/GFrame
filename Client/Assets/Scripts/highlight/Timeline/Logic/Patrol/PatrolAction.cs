using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/巡逻", typeof(PatrolAction))]
    public class PatrolAction : TimeAction
    {
        [Desc("当前坐标")]
        public IVector3 cur;
        public int curIdx;
        public int dirValue = 1;
        public NpcMapData data;
        public override TriggerStatus OnTrigger()
        {
            data = this.owner.data as NpcMapData;
            if (data == null || data.Path == null)
            {
                Debug.LogError("缺少寻路数据:" + data.Id);
                return TriggerStatus.Failure;
            }
                
            float dis = int.MaxValue;
            for(int i=0;i<data.Path.Length;i++)
            {
                float toDis = Vector3.Distance(data.Path[i], this.owner.position);
                if(toDis < dis)
                {
                    dis = toDis;
                    curIdx = i;
                    dirValue = 1;
                }
            }
            return (cur == null ) ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            if (curIdx >= data.Path.Length || curIdx < 0)
                return;
            cur.vec3 = data.Path[curIdx];
            if(Vector3.Distance(this.owner.position, cur.vec3) < 0.1f)
            {
                curIdx += dirValue;
                if (curIdx > data.Path.Length - 1)
                {
                    dirValue = -1;
                    curIdx = data.Path.Length - 2;
                }
                if(curIdx < 0)
                {
                    dirValue = 1;
                    curIdx = 1;
                }
            }
        }
        public override void OnStop()
        {
            base.OnStop();
        }
    }
}