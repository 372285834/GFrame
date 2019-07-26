using highlight;
using highlight.tl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour
{
    public Role chief;
    public GameObject player;
    public GameObject[] npcs;
    public int Count = 10;
    public int rang = 25;
    public Role monster;
    //public TimelineStyle style;
    //public KeyMoveEvent move = new KeyMoveEvent();
    // Start is called before the first frame update
    public GameObject root;
    public List<string> loopNames = new List<string>()
    {
        "run","wait_1"
    };
    void Awake()
    {
        Id.Global.generateNewId();
        Id.Global.generateNewId();
        root = new GameObject("root");
        root.transform.position = Vector3.zero;
        //style = LoadTimeStyle.Load();

        Application.targetFrameRate = App.RenderFrameRate;
        chief = CreatRoleInfo(RoleType.Player, player);
        chief.attrs.SetProp(AttrType.move_speed, 4000);
        Skills skills = Skills.Get(chief);
        SkillData data = new SkillData();
        data.id = 1;
        data.url = "skill_1";
        data.cd = new CDData(1000);
        skills.DataList.Add(data);

        chief.skills = skills;
        RoleManager.ChiefId = chief.onlyId;
        AnimationClip[] clips = chief.animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
           // Debug.Log(clips[i].name);
            if (loopNames.Contains(clips[i].name))
            {
                clips[i].wrapMode = WrapMode.Loop;
                Debug.Log(clips[i].name + "," + clips[i].isLooping);
            }

        }
        chief.ai = TimelineFactory.Creat("player_1", chief);
        chief.ai.Play(0);
    }
    public static System.Random random = new System.Random();
    void Start()
    {
        //Random.InitState(Time.renderedFrameCount);
        GameObject temp = npcs[0];
        for (int i = 0; i < Count; i++)
        {
            Vector3 pos = GetRandomPos(rang);
            Vector3 dir = GetRandomPos(80).normalized;

            GameObject npcGo = GameObject.Instantiate(temp, root.transform);
            Role npc = CreatRoleInfo(RoleType.Monster, npcGo);
            npc.SetPos(pos, true);
            npc.SetForward(dir);
            npc.attrs.SetProp(AttrType.field, 5000);
            npc.attrs.SetProp(AttrType.atk_rang, 1500);
            npc.attrs.SetProp(AttrType.atk_speed, 2000);
            NpcMapData data = new NpcMapData();
            data.Pos = pos;// npcGo.transform.position;
            data.Dir = dir;// npcGo.transform.eulerAngles;
            data.Path = new Vector3[3];
            for (int j = 0; j < 3; j++)
            {
                data.Path[j] = pos + GetRandomPos(6);
            }
            npc.data = data;
            //monster = npc;
            npc.ai = TimelineFactory.Creat("npc_ai_1", npc);
            npc.ai.Play(0);

        }
    }
    public Vector3 GetRandomPos(float radio)
    {
       // float x = Random.Range(min, max);
       // float z = Random.Range(max, max);

            var x = (float)random.NextDouble() * radio;
            var z = (float)random.NextDouble() * radio;
        return new Vector3(x, 0, z);
    }
    Role CreatRoleInfo(RoleType t,GameObject go)
    {
        Role role = RoleManager.Creat(t);
        role.SetControl(go);
        role.SetPos(go.transform.position, true);
        role.SetForward(go.transform.forward);
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
        highlight.App.Update();
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
        chief.OnDrawGizmos();
        List<Role> list = RoleManager.GetList(RoleType.Monster);
        for (int i = 0; i < list.Count; i++)
        {
            Gizmos.color = Color.white;
            list[i].OnDrawGizmos();
            NpcMapData data = list[i].data as NpcMapData;
            Vector3 start = data.Path[0];
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(data.Pos, Vector3.one * 0.3f);
            Gizmos.color = Color.black;
            for (int j = 0; j < data.Path.Length; j++)
            {
                Gizmos.DrawCube(data.Path[j], Vector3.one * 0.2f);
            }
        }
        if(showGizmosTest)
        {
            for (int i = 1; i < 100000; i++)
            {
                Gizmos.DrawCube(player.transform.position, Vector3.one);
            }
        }
        
    }
#endif
}
