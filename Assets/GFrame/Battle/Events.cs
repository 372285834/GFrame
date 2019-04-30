using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public struct RoleEvent
    {
        public ushort id;
        public short x;
        public short z;
        public int length;
        public ushort skillId;
        public short skillX;
        public short skillZ;
        public bool isMove
        {
            get { return length > 0; }
        }
        public bool isDir { get { return x != 0 || z != 0; } }
        public Vector3 dir
        {
            get
            {
                return new Vector3(x * 0.001f, 0f, z * 0.001f);
            }
        }
        public bool isSkillDir { get { return skillX != 0 || skillZ != 0; } }
        public Vector3 skillDir
        {
            get
            {
                return new Vector3(skillX * 0.001f, 0f, skillZ * 0.001f);
            }
        }
        public Vector3 GetPos(Vector3 pos)
        {
            return pos + dir * length * 0.001f;
        }
    }
    public static class Events
    {
        public static int Length { get { return Queue.Count; } }
        public static Queue<List<RoleEvent>> Queue = new Queue<List<RoleEvent>>();
        public static Dictionary<ushort, RoleEvent> Current = new Dictionary<ushort, RoleEvent>();
        public static void Update()
        {
            Current.Clear();
            if (Queue.Count > 0)
            {
                List<RoleEvent> list = Queue.Dequeue();
                for (int i = 0; i < list.Count; i++)
                {
                    Current[list[i].id] = list[i];
                }
                list.Clear();
                ListPool<RoleEvent>.Release(list);
            }
        }
        public static void Enqueue(List<RoleEvent> list)
        {
            Queue.Enqueue(list);
        }
        public static RoleEvent Add(ushort id)
        {
            if (Current.ContainsKey(id))
                return Current[id];
            RoleEvent evt = new RoleEvent();
            Current[id] = evt;
            evt.id = id;
            return evt;
        }
        public static bool Get(short id,out RoleEvent evt)
        {
            if (Current.TryGetValue((ushort)id, out evt))
                return true;
            return false;
        }
        public static bool Contains(ushort id)
        {
            return Current.ContainsKey(id);
        }
        public static RoleEvent Get(this Role role)
        {
            RoleEvent evt;
            Current.TryGetValue((ushort)role.onlyId, out evt);
            return evt;
        }
        public static void AddDir(ushort id,Vector2 dir)
        {
            RoleEvent evt = Add(id);
            evt.x = (short)Mathf.Round(dir.x * 1000);
            evt.z = (short)Mathf.Round(dir.y * 1000);
            Current[id] = evt;
        }
        public static void AddLength(ushort id, float length)
        {
            RoleEvent evt = Add(id);
            evt.length = (int)Mathf.Round(length * 1000);
            Current[id] = evt;
        }
        public static void AddMove(ushort id, Vector3 cur,Vector3 to)
        {
            Vector3 dir3 = to - cur;
            float length = dir3.sqrMagnitude;
            Vector2 dir = new Vector2(dir3.x, dir3.z);
            dir.Normalize();
            AddMove(id,dir,length);
        }
        public static void AddMove(ushort id, Vector2 dir, float length)
        {
            RoleEvent evt = Add(id);
            evt.x = (short)Mathf.Round(dir.x * 1000);
            evt.z = (short)Mathf.Round(dir.y * 1000);
            evt.length = (int)Mathf.Round(length * 1000);
            Current[id] = evt;
        }
        public static void AddSkill(ushort id, ushort skillId)
        {
            RoleEvent evt = Add(id);
            evt.skillId = skillId;
            Current[id] = evt;
        }
        public static void AddSkill(ushort id, ushort skillId, Vector2 dir)
        {
            RoleEvent evt = Add(id);
            evt.skillId = skillId;
            evt.skillX = (short)Mathf.Round(dir.x * 1000);
            evt.skillZ = (short)Mathf.Round(dir.y * 1000);
            Current[id] = evt;
        }
        public static void Clear()
        {
            Queue.Clear();
        }
    }
}