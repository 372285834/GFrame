using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/A*寻路", typeof(AStarAction))]
    public class AStarAction : TimeAction
    {
        [Desc("pos")]
        public IVector3 pos;
        [Desc("目标坐标")]
        public IVector3 to;

        public List<AStarNode> Paths = new List<AStarNode>();
        public int PathIndex;

        //public int sx;
        //public int sy;
        public int ex;
        public int ey;
        public override TriggerStatus OnTrigger()
        {
            //sx = 0;
            //sy = 0;
            PathIndex = 0;
            int X = Mathf.CeilToInt(this.owner.location.x * 0.001f);
            int Y = Mathf.CeilToInt(this.owner.location.z * 0.001f);
            AStar aStar = AStar.Inst;
            aStar.enableMapDataPixel(X, Y, false);
            ex = X;
            ey = Y;
            return TriggerStatus.Success;
        }
       // public static int FindPathFrameLength = 5;
        public static float ErrorDis = 0.01f;
        // static NavMeshPath path = new NavMeshPath();
        public override void OnUpdate()
        {
            //if ((App.frame + this.owner.onlyId) % FindPathFrameLength != 0)
            //    return;
            int X = Mathf.CeilToInt(this.owner.location.x * 0.001f);
            int Y = Mathf.CeilToInt(this.owner.location.z * 0.001f);

            Vector3 toPos = Vector3.zero;
            if (to != null)
            {
                toPos = to.vec3;
            }
            else
            {
                Role t = this.target.getObj(0);
                if (t == null)
                    return;
                toPos = t.position;
            }
            //  ProfilerTest.BeginSample("PathFindAction.OnUpdate");
            //pos.vec3 = t.position;

            AStar aStar = AStar.Inst;
            int toX = Mathf.CeilToInt(toPos.x);
            int toY = Mathf.CeilToInt(toPos.z);
            if(X == toX && Y == toY)
            {
                pos.vec3 = toPos;
                return;
            }
            if (Paths.Count == 0)
            {
                //if (aStar.canWalkPixel(toX, toY))
              //  {
                    //sx = 0;
                    //sy = 0;
                    ProfilerTest.BeginSample("findPathPixel");
                    aStar.findPathPixel(Paths,X, Y, toX, toY);
                    ProfilerTest.EndSample();
                    PathIndex = 0;
            //    }
               // else
               // {
                   // pos.vec3 = toPos;
                 //   return;
              //  }
                    
            }

            if(Paths.Count > 0)
            {
                if (PathIndex > 0)
                {
                    float dis = Vector3.Distance(pos.vec3, this.owner.position);
                    if (dis > ErrorDis)
                        return;
                }
                if (PathIndex < Paths.Count && SetTarget(X, Y))
                {
                }
                else
                {
                    //sx = 0;
                    //sy = 0;
                    //if (toX == ex && toY == ey)
                    //    return;
                    //ex = toX;
                    //ey = toY;
                    ProfilerTest.BeginSample("findPathPixel");
                    aStar.findPathPixel(Paths, X, Y, toX, toY);
                    ProfilerTest.EndSample();
                    PathIndex = 0;
                    SetTarget(X, Y, true);
                }
          //      else
        //        {
         //           Paths.Clear();
         //           PathIndex = 0;
                    //sx = 0;
                    //sy = 0;
                    //ex = 0;
                    //ey = 0;
        //            pos.vec3 = toPos;
        //        }
            }
        }
        bool curEndPosCanMove
        {
            get
            {
                int tx = Paths[PathIndex].X;
                int ty = Paths[PathIndex].Y;
                bool b = AStar.Inst.canWalkPixel(tx, ty);
                return b;
            }
        }
        bool SetTarget(int  X,int Y,bool force = false)
        {
            bool b = force || curEndPosCanMove;
            if (b)
            {
                //sx = X;
                //sy = Y;
                //X = tx;
                //Y = ty;
                AStar aStar = AStar.Inst;
                aStar.enableMapDataPixel(ex, ey, true);
                int tx = Paths[PathIndex].X;
                int ty = Paths[PathIndex].Y;
                aStar.enableMapDataPixel(tx, ty, false);
                ex = tx;
                ey = ty;
                pos.vec3 = new Vector3(tx - 0.5f, 0f, ty - 0.5f);
                PathIndex++;
            }
            return b;
        }
        public override void OnStop()
        {
            AStar.Inst.enableMapDataPixel(ex, ey, true);
            Paths.Clear();
            base.OnStop();
        }
        public override void OnDrawGizmos()
        {
            if(Paths.Count > 0)
            {
                Gizmos.color = PathIndex == 0?Color.blue: Color.white;
                Vector3 start = new Vector3(Paths[0].X - 0.5f, 0f, Paths[0].Y - 0.5f);
                Gizmos.DrawSphere(start, 0.2f);
                for (int i=1;i<Paths.Count;i++)
                {
                    Gizmos.color = PathIndex == i ? Color.blue : Color.white;
                    Vector3 end = new Vector3(Paths[i].X - 0.5f, 0f, Paths[i].Y - 0.5f);
                    Gizmos.DrawLine(start, end);
                    start = end;
                    Gizmos.DrawSphere(start, 0.2f);
                }
            }
        }
    }
}