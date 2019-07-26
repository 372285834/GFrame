using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/寻路", typeof(PathFindAction))]
    public class PathFindAction : TimeAction
    {
        [Desc("寻路参数")]
        public PathFindData data;
        [Desc("pos")]
        public IVector3 pos;

        public override TriggerStatus OnTrigger()
        {
            return TriggerStatus.Success;
        }
        public static int FindPathFrameLength = 5;
        public static float ErrorDis = 0.03f;
       // static NavMeshPath path = new NavMeshPath();
        public override void OnUpdate()
        {
            if ((App.frame + this.owner.onlyId) % FindPathFrameLength != 0)
                return;
            Role t = this.target.getObj(0);
            if (t == null)
                return;
          //  ProfilerTest.BeginSample("PathFindAction.OnUpdate");
            pos.vec3 = t.position;
            Vector3 start = this.owner.position;
            bool b = false;
            if (data.mStyle.isAll)
                b = UnityEngine.AI.NavMesh.CalculatePath(start, t.position, UnityEngine.AI.NavMesh.AllAreas, data.path);
            else
                b = UnityEngine.AI.NavMesh.CalculatePath(start, t.position, data.filter, data.path);
            if(!b || data.path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                //ProfilerTest.EndSample();
                return;
            }
            Vector3[] path = data.path.corners;
            if (path.Length > 0)
            {
                for (int i = 0; i < path.Length; i++)
                {
                    if (Vector3.Distance(path[i], start) > ErrorDis)
                    {
                        pos.vec3 = path[i];
                        break;
                    }
                }
            }
         //   ProfilerTest.EndSample();
            //Vector3 pos = eva.Evaluate(this.start.pos, this.end.pos, this.timeObject.progress);
            // this.role.SetPos(pos, false);
        }
    }
}