using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public partial class Role
    {
        public NpcMapData npcMapData
        {
            get
            {
                return this.data as NpcMapData;
            }
        }
        public float move_speed
        {
            get
            {
                float speed = this.attrs.GetFloat(AttrType.move_speed, false);
                float length = speed * App.logicDeltaTime;
                return length;
            }
        }
        public float ClipSpeed
        {
            get
            {
                float speed = 1f;
                RoleState state = this.state;
                if (state == RoleState.Move)
                {
                    speed = this.attrs.GetFloat(AttrType.move_speed, false, 0);
                    //RVO.Vector2 vel = RVO.Simulator.Instance.getAgentVelocity(this.onlyId);
                    //speed *= (vel.x() + vel.y()) / App.logicDeltaTime;
                }
                //else if (state == RoleState.Attack)
                //{
                //    speed = this.attrs.GetFloat(AttrType.atk_speed, false, 0);
                //}
                // speed = Mathf.Log(speed, 3);
                if (speed > 1f)
                {
                    speed = speed > 10f ? 10f : speed;
                    speed = 1 + (speed - 1) * 0.1f;
                    //speed = Mathf.Pow(speed, 0.5f);
                }
                return speed;
            }
        }
        public bool CanPlayHit
        {
            get
            {
                RoleState state = this.state;
                bool b = state == RoleState.Idle || state == RoleState.Move;
                b = b && !this.attrs.GetBoolV(AttrType.non_move);
                return b;
            }
        }

        public bool non_move
        {
            get
            {
                return this.attrs.GetBoolV(AttrType.non_move);
            }
        }
        public bool non_obstacle
        {
            get
            {
                return this.attrs.GetBoolV(AttrType.non_obstacle);
            }
        }
    }
}