using RVO;
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
        RVO.Vector2 vel;
        //public static float RunSpeed = 1;
        bool isTemp = false;
        Vector3 temp;
        int tempLength = 2;
        static float errorDis = 0.03f;
        public override TriggerStatus OnTrigger()
        {
           // this.owner.PlayClip("run",true);
            this.owner.Switch(RoleState.Move);
            return pos == null ? TriggerStatus.Failure : TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            if (this.owner.non_move)
                return;
            bool non_obstacle = this.owner.non_obstacle;
            if(non_obstacle)
            {
                ForceMove();
                return;
            }
            Vector3 start = this.owner.position;
            Vector3 end = pos.vec3;
            int id = this.owner.onlyId;
            if (isTemp)
            {
                if (Vector3.Distance(start, temp) < errorDis)
                {
                    isTemp = false;
                    //tempLength = 1;
                }
                else
                    end = temp;
            }

                RVO.Vector2 lastPrefVel = RVO.Simulator.Instance.getAgentPrefVelocity(id);
                RVO.Vector2 lastVel = Simulator.Instance.getAgentVelocity(id);
                if (lastPrefVel.IsValid() && !lastVel.IsValid())
                {
                    Vector3 newDir = Vector3.Cross((end - start), Vector3.up).normalized;
                    temp = start + newDir * tempLength;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(temp, out hit, 0.5f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        isTemp = true;
                        temp = hit.position;// + hit.normal * 0.5f;
                        end = temp;
                    }
                }
            


            // this.owner.SetClipSpeed(speed/RunSpeed);
            float length = this.owner.move_speed;
            Vector3 dir = (end - start).normalized;

            float dis = Vector3.Distance(start, end);
            if (length > dis)
                length = dis;
            Vector3 to = start + dir * length;

            start.y = to.y;
            this.owner.SetPos(start, false);
            vel = new RVO.Vector2(dir.x, dir.z) * length;
            RVO.Simulator.Instance.setAgentPrefVelocity(id, vel);
            RVO.Simulator.Instance.setAgentMaxSpeed(id, length);
        }
        void ForceMove()
        {
            Vector3 start = this.owner.position;
            Vector3 end = pos.vec3;
            float length = this.owner.move_speed;
            Vector3 dir = (end - start).normalized;

            float dis = Vector3.Distance(start, end);
            if (length > dis)
                length = dis;

            Vector3 to = start + dir * length;
            this.owner.SetPos(to, false);
            Simulator.Instance.setAgentPosition(owner.onlyId, new RVO.Vector2(to.x, to.z));
            RVO.Simulator.Instance.setAgentMaxSpeed(owner.onlyId, 0f);
            if ((VInt3)dir != this.owner.forward)
            {
                //// Debug.Log(((VInt3)dir).ToString() + "," + this.owner.forward.ToString());
             //   dir = SetRotationAction.GetForward(start, end, (Vector3)this.owner.forward);
                this.owner.SetForward(dir,false);
            }
        }
        public override void OnFinish()
        {
            isTemp = false;
            RVO.Simulator.Instance.setAgentPrefVelocity(owner.onlyId, RVO.Vector2.Zero);
            RVO.Simulator.Instance.setAgentMaxSpeed(owner.onlyId, 0f);
            base.OnFinish();
        }
        public override void OnDrawGizmos() {
            if(pos != null)
            {
                Vector3 start = this.owner.position;
                Gizmos.color = Color.red;
                if (isTemp)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(start, temp);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(start, pos.vec3);
                }
                    
            }
        }
    }
}