using highlight;
using highlight.tl;
using RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITest : MonoBehaviour
{
    public Role chief;
    public GameObject player;
    public GameObject[] npcs;
    public int Count = 10;
    public int rang = 25;
    public float rang2 = 20f;
    public Role monster;
    //public TimelineStyle style;
    //public KeyMoveEvent move = new KeyMoveEvent();
    // Start is called before the first frame update
    public GameObject root;
    public Transform itemRoot;
    GameObjectPool<RoleControl> pool;
    public List<string> loopNames = new List<string>()
    {
        "run","wait_1"
    };
    void Awake()
    {
        Simulator.Instance.setTimeStep(1f);
        Simulator.Instance.setAgentDefaults(2f, 8, 1.0f, 1.0f, 0.5f, 1.0f, new RVO.Vector2(0.0f, 0.0f));
        // add in awake
        if(itemRoot != null)
        {
            MeshRenderer[] mrs = itemRoot.GetComponentsInChildren<MeshRenderer>(); 
            for(int i=0;i<mrs.Length;i++)
            {
                //Vector3 pos = mrs[i].transform.position;
                Bounds bounds = mrs[i].bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;
                IList<RVO.Vector2> obstacle4 = new List<RVO.Vector2>();
                obstacle4.Add(new RVO.Vector2(max.x, max.z));
                obstacle4.Add(new RVO.Vector2(min.x, max.z));
                obstacle4.Add(new RVO.Vector2(min.x, min.z));
                obstacle4.Add(new RVO.Vector2(max.x, min.z));
                Simulator.Instance.addObstacle(obstacle4);
            }
        }
        Simulator.Instance.processObstacles();

        // AStar.Inst.initMapData(200, 200, 1);
        Id.Global.generateNewId();
        Id.Global.generateNewId();
        root = new GameObject("root");
        root.transform.position = Vector3.zero;
        pool = new GameObjectPool<RoleControl>(npcs[0], null, null, root.transform, true);
        //style = LoadTimeStyle.Load();

        Application.targetFrameRate = App.RenderFrameRate;
        chief = CreatRoleInfo(RoleType.Player, player);
        Simulator.Instance.addAgent(chief.onlyId, new RVO.Vector2(chief.position.x, chief.position.z));
        chief.control.InitClipInfo();
        chief.attrs.SetProp(AttrType.move_speed, 5000);
        chief.attrs.SetBool(AttrType.non_obstacle, true);
        Skills skills = Skills.Get(chief);
        SkillData data = new SkillData();
        data.id = 1;
        data.url = "skill_1";
        data.cd = new CDData(1000);
        skills.DataList.Add(data);

        chief.skills = skills;
        RoleManager.ChiefId = chief.onlyId;
        //AnimationClip[] clips = chief.animator.runtimeAnimatorController.animationClips;
        //for (int i = 0; i < clips.Length; i++)
        //{
        //   // Debug.Log(clips[i].name);
        //    if (loopNames.Contains(clips[i].name))
        //    {
        //        clips[i].wrapMode = WrapMode.Loop;
        //        Debug.Log(clips[i].name + "," + clips[i].isLooping);
        //    }

        //}
        chief.ai = TimelineFactory.Creat("player_1", chief);
        chief.ai.Play(0);
    }
    public static System.Random random = new System.Random();
    void Start()
    {
        //Random.InitState(Time.renderedFrameCount);
        for (int i = 0; i < Count; i++)
        {
            CreatNPC();
        }
    }
    List<Vector3> tempList = new List<Vector3>();
    void CreatNPC()
    {
        tempList.Clear();
        Vector3 pos = GetRandomPos(rang);
        tempList.Add(pos);
        Vector3 end = pos;
        for (int j = 0; j < 3; j++)
        {
            end = GetRandomPos2(tempList, pos, end, rang2);
        }
        pos = tempList[tempList.Count / 2];
        Vector3 dir = GetRandomPos(80).normalized;
        RoleControl npcGo = pool.Get();
        ProfilerTest.BeginSample("RoleManager.Creat");
        Role npc = CreatRoleInfo(RoleType.Monster, npcGo.gameObject);
        npc.control.InitClipInfo();
        npc.SetPos(pos, true);
        npc.SetForward(dir,true);
        Simulator.Instance.addAgent(npc.onlyId, new RVO.Vector2(pos.x, pos.z));
        npc.attrs.SetProp(AttrType.field, 5000);
        npc.attrs.SetProp(AttrType.atk_rang, 1500);
        npc.attrs.SetProp(AttrType.atk_speed, 5000);
        ProfilerTest.EndSample();
        //monster = npc;
        npc.ai = TimelineFactory.Creat("npc_ai_1", npc);
        NpcMapData data = new NpcMapData();
        data.Pos = pos;// npcGo.transform.position;
        data.Dir = dir;// npcGo.transform.eulerAngles;
        data.Path = tempList.ToArray();
        npc.data = data;
        npc.ai.Play(0);
    }
    NavMeshPath path;
    public Vector3 GetRandomPos2(List<Vector3> list,Vector3 source,Vector3 pos, float radio)
    {
        path = new NavMeshPath();
        var x = (float)random.NextDouble() * radio;
        var z = (float)random.NextDouble() * radio;
        Vector3 to = source + new Vector3(x, 0, z);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(to, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
        {
            to = hit.position;// + hit.normal * 0.5f;
        }
        if (NavMesh.CalculatePath(pos, to, NavMesh.AllAreas, path) && path.corners.Length > 0)
        {
            if(path.corners.Length > 1)
            {
                for (int i = 1; i < path.corners.Length - 2; i++)
                {
                    bool result = NavMesh.FindClosestEdge(path.corners[i], out hit, NavMesh.AllAreas);
                    if (result && hit.distance < 0.5f)
                        path.corners[i] = hit.position + hit.normal * 0.5f;
                }
            }
            for (int i = 0; i < path.corners.Length; i++)
            {
                list.Add(path.corners[i]);
            }
        }
        else
        {
            list.Add(to);
            return to;
        }
        return list[list.Count -1];
    }
    public Vector3 GetRandomPos(float radio)
    {
        // float x = Random.Range(min, max);
        // float z = Random.Range(max, max);
        Vector3 pos = Vector3.zero;
            var x = (float)random.NextDouble() * radio;
            var z = (float)random.NextDouble() * radio;
        pos = new Vector3(x, 0, z);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
        {
            pos = hit.position;// + hit.normal * 0.5f;
        }
        else
        {

        }
        return pos;
    }
    Role CreatRoleInfo(RoleType t,GameObject go)
    {
        Role role = RoleManager.Creat(t);
        role.SetControl(go);
        role.SetPos(go.transform.position, true);
        role.SetForward(go.transform.forward, true);
        role.attrs = RoleAttrs.Get(role);
        role.attrs.SetProp(AttrType.move_speed, 2000);
        role.attrs.SetProp(AttrType.hp, 10000);
        role.attrs.SetProp(AttrType.max_hp, 10000);
       // role.Switch(RoleState.Idle);
        return role;
    }
    // Update is called once per frame
    void Update()
    {
        App.Update();
    }
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            CreatNPC();
        }
        if(Input.GetKeyDown(KeyCode.G))
        {
            Role r = RoleManager.roleDic[(int)RoleType.Monster][0];
            pool.Release(r.control);
            ProfilerTest.BeginSample("RoleManager.Destroy");
            RoleManager.Destroy(r.onlyId);
            ProfilerTest.EndSample();
        }
        App.LateUpdate();
    }
    public void SetTimeline(Timeline tl)
    {
        //if(monster != null)
        //{
        //    monster.ai = tl;
        //    tl.owner = monster;
        //}
        
        //foreach (var role in RoleManager.dic.Values)
        //{
        //    if(role.type == RoleType.Monster)
        //    {
        //        role.ai = tl;
        //    }
        //}
    }

    public bool showGizmosTest = false;
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (chief == null)
            return;
        RoleManager.OnDrawGizmos();
       // chief.OnDrawGizmos();
     //   List<Role> list = RoleManager.GetList(RoleType.Monster);
    //    for (int i = 0; i < list.Count; i++)
   //     {
       //     Gizmos.color = Color.white;
      //      list[i].OnDrawGizmos();
            //NpcMapData data = list[i].data as NpcMapData;
            //Vector3 start = data.Path[0];
            //Gizmos.color = Color.blue;
            //Gizmos.DrawCube(data.Pos, Vector3.one * 0.3f);
            //Gizmos.color = Color.black;
            //for (int j = 0; j < data.Path.Length; j++)
            //{
            //    Gizmos.DrawCube(data.Path[j], Vector3.one * 0.2f);
            //}
  //      }
        if (showGizmosTest)
        {
            //for (int i = 1; i < 100000; i++)
            //{
            //    Gizmos.DrawCube(player.transform.position, Vector3.one);
            //}
        }

        //bool[,] mapData = AStar.Inst.MapData;
        //Gizmos.color = Color.black * 0.5f;
        //for (int w = 0; w < AStar.Inst.MapWidth; ++w)
        //{
        //    for (int h = 0; h < AStar.Inst.MapHeight; ++h)
        //    {
        //        if (!mapData[w, h])
        //        {
        //            Gizmos.DrawCube(new Vector3(w - 0.5f, 0, h - 0.5f), Vector3.one);
        //        }
        //    }
        //}
        
    }
#endif
}
