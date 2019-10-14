using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace highlight
{
    public class KeyMoveEvent
    {
        public KeyCode up = KeyCode.W;
        public KeyCode down = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        public Vector2 dir;
        public bool isKey = false;
        public void Update()
        {
            isKey = false;
            dir = Vector2.zero;
            if (Input.GetKey(up))
            {
                dir += Vector2.up;
                isKey = true;
            }
            else if (Input.GetKey(down))
            {
                dir += Vector2.down;
                isKey = true;
            }
            if (Input.GetKey(left))
            {
                dir += Vector2.left;
                isKey = true;
            }
            else if (Input.GetKey(right))
            {
                dir += Vector2.right;
                isKey = true;
            }
            Role role = RoleManager.Chief;
            if (role.attrs.GetBoolV(AttrType.non_control))
                return;
            if (isKey && !role.non_move)
            {
                dir.Normalize();
                
                float speed = role.move_speed;
                Vector3 to = role.position + new Vector3(dir.x, 0f, dir.y) * speed;
                NavMeshHit hit;
                if(NavMesh.SamplePosition(to, out hit, 1f,UnityEngine.AI.NavMesh.AllAreas))
                {
                    //Events.AddMove(RoleManager.ChiefId, hit.position);
                    Events.AddDir(RoleManager.ChiefId, (hit.position  - role.position).ToVector2());
                }
            }
            if(Input.GetKeyDown(KeyCode.R))
            {
                SkillData data = role.skills.GetData(1);
                if (!role.skills.CanPlaySkill(data))
                    return;
                //  Debug.Log(data.cd.ToString());
                Events.AddSkill(RoleManager.ChiefId, data.id, role.position);
            }
        }
    }
}