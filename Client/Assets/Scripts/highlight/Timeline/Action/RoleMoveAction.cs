using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace highlight.tl
{
    [Action("行为/简单人物移动", typeof(RoleMoveAction))]
    public class RoleMoveAction : TimeAction
    {
        [Desc("目的地")]
        public IVector3 pos;
      //  Vector3 temp;
       // bool isTemp = false;
       // int temLength = 1;
        public static float RunSpeed = 1;
        public override TriggerStatus OnTrigger()
        {
            this.owner.PlayClip("run",true);
            return pos == null ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            bool nonMove = this.owner.attrs.GetBoolV(AttrType.non_move);
            if (nonMove)
                return;
            float speed = this.owner.attrs.GetFloat(AttrType.move_speed, false);
            this.owner.SetClipSpeed(speed/RunSpeed);
            float length = speed * App.logicDeltaTime;
            Vector3 start = this.owner.position;
            Vector3 end = pos.vec3;
            //if (isTemp)
            //{
            //    if (Vector3.Distance(start, temp) < errorDis)
            //    {
            //        temLength = 1;
            //        isTemp = false;
            //    }
            //    else
            //        end = temp;
            //}
            float dis = Vector3.Distance(start, end);
            if (length > dis)
                length = dis;
            Vector3 dir = (end - start).normalized;
            Vector3 to = start + dir * length;
            this.owner.SetPos(to, false);
            /*
            ProfilerTest.BeginSample("NavMesh.SamplePosition");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(to, out hit, length, UnityEngine.AI.NavMesh.AllAreas))
            {
                Vector3 result = hit.position;
               // result.y = 0f;
                this.owner.SetPos(result, false);

                float dis2 = Vector3.Distance(result, start);
                if (dis2 * 2f <= length)
                {
                    //Vector3 newDir = Vector3.Cross(dir, Vector3.up);
                    //if (temLength % 3 == 0)
                    //    newDir += dir;
                    //temp = start + newDir.normalized * temLength;
                    //isTemp = true;
                    //temLength++;
                    ProfilerTest.BeginSample("NavMesh.CalculatePath");
                    if (UnityEngine.AI.NavMesh.CalculatePath(start, pos.vec3, UnityEngine.AI.NavMesh.AllAreas, path))
                    {
                        if(path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 0)
                        {
                            for(int i=0;i< path.corners.Length;i++)
                            {
                                if(Vector3.Distance(path.corners[i], start) > errorDis)
                                {
                                    temp = path.corners[i];
                                    isTemp = true;
                                    break;
                                }
                            }
                        }
                        path.ClearCorners();
                    }
                    ProfilerTest.EndSample();
                    //  Debug.Log(dis2 + "," + length + "," + hit.distance);
                }

            }
            else
                return;
            ProfilerTest.EndSample();
            */
            if ((VInt3)dir == this.owner.forward)
                return;
           // Debug.Log(((VInt3)dir).ToString() + "," + this.owner.forward.ToString());

            //Quaternion curQ = Quaternion.LookRotation((Vector3)this.owner.forward);
            //Quaternion toQ = Quaternion.LookRotation(dir);// Quaternion.LookRotation(end - start);
            //Quaternion q = Quaternion.RotateTowards(curQ, toQ, App.logicDeltaTime * SetRotationAction.RotateSpeed);
            Vector3 forward = SetRotationAction.GetForward(start, end, (Vector3)this.owner.forward, speed);
            // Vector3 fwd = toQ * Vector3.forward;
            this.owner.SetForward(forward);
        }

        public override void OnDrawGizmos() {
            if(pos != null)
            {
                Vector3 start = this.owner.position;
                //if(isTemp)
                //{
                //    Gizmos.color = Color.red;
                //    Gizmos.DrawLine(start, temp);
                //}
                //else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(start, pos.vec3);
                }
                    
            }
        }
    }
}