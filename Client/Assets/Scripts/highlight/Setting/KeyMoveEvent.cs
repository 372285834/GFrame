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
            if (isKey)
            {
                dir.Normalize();
                Events.AddDir(RoleManager.ChiefId, dir);
                float speed = role.attrs.GetFloat(AttrType.move_speed, true);
                Vector3 to = role.position + new Vector3(dir.x, 0f, dir.y) * speed * App.logicDeltaTime;
                NavMeshHit hit;
                if(NavMesh.SamplePosition(to, out hit, 1f,UnityEngine.AI.NavMesh.AllAreas))
                {
                    Events.AddMove(RoleManager.ChiefId, hit.position);
                }
            }
            if(Input.GetKeyDown(KeyCode.R))
            {
                SkillData data = role.skills.GetData(1);
              //  Debug.Log(data.cd.ToString());
                if (data.cd.IsComplete)
                {
                    data.cd.Reset();
                    Events.AddSkill(RoleManager.ChiefId, data.id, role.position);
                }
            }
        }
    }
}