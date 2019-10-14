using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public struct RoleEvent
    {
        public int id;
        public int selectId;
        public short dirX;
        public short dirZ;
        public int moveX;
        public int moveZ;
        public int skillId;
        public int skillX;
        public int skillZ;
        public bool isMove { get { return moveX != 0 || moveZ != 0; } }
        public bool isDir { get { return dirX != 0 || dirZ != 0; } }
        public Vector3 pos
        {
            get
            {
                return new Vector3(moveX * 0.001f, 0f, moveZ * 0.001f);
            }
        }
        public Vector3 dir
        {
            get
            {
                return new Vector3(dirX * 0.01f, 0f, dirZ * 0.01f);
            }
        }
        public bool isSkillPos { get { return skillX != 0 || skillZ != 0; } }
        public Vector3 skillPos
        {
            get
            {
                return new Vector3(skillX * 0.001f, 0f, skillZ * 0.001f);
            }
        }
    }
    public static class Events
    {
        public static int Length { get { return Queue.Count; } }
        public static Queue<List<RoleEvent>> Queue = new Queue<List<RoleEvent>>();
        public static Dictionary<int, RoleEvent> Current = new Dictionary<int, RoleEvent>();
        public static Dictionary<int, RoleEvent> LastDic = new Dictionary<int, RoleEvent>();
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
        public static void Enqueue()
        {
            List<RoleEvent> list = ListPool<RoleEvent>.Get();
            foreach (var evt in LastDic.Values)
            {
                list.Add(evt);
            }
            Queue.Enqueue(list);
            LastDic.Clear();
        }
        public static RoleEvent Add(int id)
        {
            if (LastDic.ContainsKey(id))
                return LastDic[id];
            RoleEvent evt = new RoleEvent();
            LastDic[id] = evt;
            evt.id = id;
            return evt;
        }
        public static bool Get(int id,out RoleEvent evt)
        {
            if (LastDic.TryGetValue((int)id, out evt))
                return true;
            return false;
        }
        public static bool Contains(int id)
        {
            return Current.ContainsKey(id);
        }
        public static RoleEvent Get(this Role role)
        {
            RoleEvent evt;
            Current.TryGetValue(role.onlyId, out evt);
            return evt;
        }
        public static void AddDir(int id,Vector2 dir)
        {
            RoleEvent evt = Add(id);
            evt.dirX = (short)Mathf.Round(dir.x * 100);
            evt.dirZ = (short)Mathf.Round(dir.y * 100);
            LastDic[id] = evt;
        }
        public static void AddMove(int id, Vector3 pos)
        {
            RoleEvent evt = Add(id);
            evt.moveX = (int)Mathf.Round(pos.x * 1000);
            evt.moveZ = (int)Mathf.Round(pos.z * 1000);
            LastDic[id] = evt;
        }
        public static void AddSkill(int id, int skillId)
        {
            RoleEvent evt = Add(id);
            evt.skillId = skillId;
            LastDic[id] = evt;
        }
        public static void AddSkill(int id, int skillId, Vector3 pos)
        {
            RoleEvent evt = Add(id);
            evt.skillId = skillId;
            evt.skillX = (int)Mathf.Round(pos.x * 1000);
            evt.skillZ = (int)Mathf.Round(pos.z * 1000);
            LastDic[id] = evt;
        }
        public static void Clear()
        {
            Queue.Clear();
        }
    }
}