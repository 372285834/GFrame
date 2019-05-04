using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if(isKey)
            {
                dir.Normalize();
                Events.AddDir((ushort)RoleManager.ChiefId, dir);
            }
        }
    }
}