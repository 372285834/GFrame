using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public static class RoleManager
    {
        private static ObjectPool<Role> pool = new ObjectPool<Role>();
        public static Dictionary<ulong, Role> dic = new Dictionary<ulong, Role>();

        public static ulong ChiefId;
        public static Role Chief { get { return Get(ChiefId); } }
        public static Role Get(ulong id)
        {
            Role role = null;
            dic.TryGetValue(id, out role);
            return role;
        }

        public static Role Creat(RoleType t)
        {
            Role role = pool.Get();
            role.SetOnlyId(Id.Global.generateNewId());
            dic[role.onlyId] = role;
            return role;
        }

        public static void Update(int delta)
        {
            foreach(var role in dic.Values)
            {
                role.UpdateFrame(delta);
            }
        }
        public static void UpdateRender(float interpolation)
        {
            foreach (var role in dic.Values)
            {
                role.UpdateRender(interpolation);
            }
        }
        public static void Delete(ulong id)
        {
            if (dic.ContainsKey(id))
            {
                Role role = dic[id];
                role.Clear();
                dic.Remove(id);
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
        }
    }
}