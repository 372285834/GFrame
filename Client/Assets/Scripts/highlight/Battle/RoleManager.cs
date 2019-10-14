using RVO;
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
        static List<KeyValuePair<float, int>> TempFindList = new List<KeyValuePair<float, int>>();
       // static List<int> TempFindList = new List<int>();
        public static void FindInOut(Target target, RoleType t, Vector3 pos, float rang, int num)
        {
            //List<Role> list = roleDic[(int)t];
            List<int> curList = target.mObjects;
            List<int> inList = target.inObjects;
            List<int> outList = target.outObjects;
            inList.Clear();
            outList.Clear();
            outList.AddRange(curList);
            curList.Clear();
             RVO.Simulator.Instance.getPosNeighbors(TempFindList, new RVO.Vector2(pos.x, pos.z), rang);
            for(int i=0;i< TempFindList.Count;i++)
            {
                int id = TempFindList[i].Value;
                Role r = dic[id];
                if (r.type != t)
                    continue;
                curList.Add(id);
                if (curList.Count >= num)
                    break;
            }
            //for (int i = 0; i < list.Count; i++)
            //{
            //    int id = list[i].onlyId;
            //    float dis = Vector3.Distance(pos, list[i].position);
            //    if (dis <= rang)
            //    {
            //        curList.Add(id);
            //        if (curList.Count >= num)
            //            break;
            //    }
            //}
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
            foreach (var role in dic.Values)
            {
                if(role.CanDestroy)
                {
                    DestroyList.Add(role);
                    Simulator.Instance.delAgent(role.onlyId);
                }
                else
                {
                    role.UpdateFrame(delta);
                    //UpdateAgentMaxSpeed(role);
                }
            }
            if(DestroyList.Count > 0)
            {
                for(int i=0;i< DestroyList.Count;i++)
                {
                    Destroy(DestroyList[i].onlyId);
                }
            }
            DestroyList.Clear();
            UpdateRVO();
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
                role.Destroy();
                dic.Remove(id);
                roleDic[(int)t].Remove(role);
                pool.Release(role);
            }
        }

        public static void Clear()
        {
            foreach (var role in dic.Values)
            {
                role.Destroy();
                pool.Release(role);
            }
            dic.Clear();
            roleDic.Clear();
        }
        //public static void UpdateAgentMaxSpeed(Role role)
        //{
        //    if (role.state == RoleState.Move)
        //    {
        //        float speed = role.attrs.GetFloat(AttrType.move_speed, false);
        //        float length = speed * App.logicDeltaTime;
        //        RVO.Simulator.Instance.setAgentMaxSpeed(role.onlyId, length);
        //    }
        //    else
        //    {
        //        RVO.Simulator.Instance.setAgentMaxSpeed(role.onlyId, 0f);
        //    }
        //}
        public static void UpdateRVO()
        {
            ProfilerTest.BeginSample("SimulatorROV");
            Simulator.Instance.doStep2();
            ProfilerTest.EndSample();
            ProfilerTest.BeginSample("SetROV_Pos_Forward");
            foreach (var role in dic.Values)
            {
                int sid = role.onlyId;
                if (role.state == RoleState.Move && !role.non_obstacle)
                {
                    RVO.Vector2 pos = Simulator.Instance.getAgentPosition(sid);
                    VInt3 newPos = new VInt3(pos.x(), role.position.y, pos.y());
                    if (newPos != role.location)
                    {
                        role.SetPosVInt3(newPos, false);
                        RVO.Vector2 vel = Simulator.Instance.getAgentVelocity(sid);//getAgentPrefVelocity(sid);
                        if (vel.IsValid())
                        {
                            Vector3 dir = new Vector3(vel.x(), 0, vel.y()).normalized;
                            role.SetForward(dir, false);
                        }
                    }
                    //  else
                    //    {
                    // Debug.Log("0move" + role.onlyId);
                    //  }
                }
            }
            ProfilerTest.EndSample();
        }
        public static void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            foreach (var role in dic.Values)
            {
                
                Gizmos.color = Color.white;
                role.OnDrawGizmos();
                //Gizmos.color = Color.blue;
                //   RVO.Vector2 vel = Simulator.Instance.getAgentVelocity(role.onlyId);
                Debug.DrawRay(role.position, (Vector3)role.forward, Color.blue);
            }
        }
    }
}