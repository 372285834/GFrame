using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public static class RoleManager
    {
        private static ObjectPool<Role> pool = new ObjectPool<Role>();
        public static Dictionary<int, Role> dic = new Dictionary<int, Role>();
        static Dictionary<int, List<Role>> m_roleDic;
        public static Dictionary<int, List<Role>> roleDic
        {
            get
            {
                if(m_roleDic == null)
                {
                    m_roleDic = new Dictionary<int, List<Role>>();
                    for (RoleType t = RoleType.Player; t < RoleType.Build; t++)
                    {
                        m_roleDic[(int)t] = new List<Role>();
                    }
                }
                return m_roleDic;
            }
        }
        public static int ChiefId;
        public static Role Chief { get { return Get(ChiefId); } }
        public static Role Get(int id)
        {
            Role role = null;
            dic.TryGetValue(id, out role);
            return role;
        }
        public static Role Creat(RoleType t)
        {
            Role role = pool.Get();
            role.type = t;
            role.SetOnlyId(Id.Global.generateNewId());
            dic[role.onlyId] = role;
            roleDic[(int)t].Add(role);
            return role;
        }
        public static Role Find(RoleType t,Vector3 pos, float max)
        {
            float cur = int.MaxValue;
            Role result = null;
            List<Role> list = roleDic[(int)t];
            for(int i=0;i< list.Count;i++)
            {
                float dis = Vector3.Distance(pos, list[i].position);
                if (cur > dis && dis <= max)
                {
                    cur = dis;
                    result = list[i];
                }
            }
            return result;
        }
        public static void Finds(List<Role> result,RoleType t, Vector3 pos, float rang,int num)
        {
            int cur = 0;
            List<Role> list = roleDic[(int)t];
            for (int i = 0; i < list.Count; i++)
            {
                if (cur >= num)
                    break;
                float dis = Vector3.Distance(pos, list[i].position);
                if (dis <= rang)
                {
                    cur++;
                    result.Add(list[i]);
                }
            }
        }
       // static List<int> TempFindList = new List<int>();
        public static void FindInOut(Target target, RoleType t, Vector3 pos, float rang, int num)
        {
            List<Role> list = roleDic[(int)t];
            List<int> curList = target.mObjects;
            List<int> inList = target.inObjects;
            List<int> outList = target.outObjects;
            inList.Clear();
            outList.Clear();
            outList.AddRange(curList);
            curList.Clear();
           // TempFindList.Clear();
        //    TempFindList.AddRange(curList);
            for (int i = 0; i < list.Count; i++)
            {
                int id = list[i].onlyId;
                float dis = Vector3.Distance(pos, list[i].position);
                if (dis <= rang)
                {
                    curList.Add(id);
                    if (curList.Count >= num)
                        break;
                }
            }
            for(int i=0;i< curList.Count;i++)
            {
                int id = curList[i];
                if (!outList.Remove(id))
                {
                    inList.Add(id);
                }
            }
            
        }
        public static List<Role> GetList(RoleType t)
        {
            return roleDic[(int)t];
        }
        static List<Role> DestroyList = new List<Role>();
        public static void Update(int delta)
        {
            foreach(var role in dic.Values)
            {
                if(role.CanDestroy)
                {
                    DestroyList.Add(role);
                }
                else
                    role.UpdateFrame(delta);
            }
            if(DestroyList.Count > 0)
            {
                for(int i=0;i< DestroyList.Count;i++)
                {
                    Destroy(DestroyList[i].onlyId);
                }
            }
            DestroyList.Clear();
        }
        public static void UpdateRender(float interpolation)
        {
            foreach (var role in dic.Values)
            {
                role.UpdateRender(interpolation);
            }
        }
        public static void Destroy(int id)
        {
            if (dic.ContainsKey(id))
            {
                Role role = dic[id];
                RoleType t = role.type;
                role.Clear();
                dic.Remove(id);
                roleDic[(int)t].Remove(role);
                pool.Release(role);
            }
        }

        public static void Clear()
        {
            foreach (var role in dic.Values)
            {
                role.Clear();
                pool.Release(role);
            }
            dic.Clear();
            roleDic.Clear();
        }
    }
}