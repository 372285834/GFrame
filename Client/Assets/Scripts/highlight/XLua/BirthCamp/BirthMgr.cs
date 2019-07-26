using highlight;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using XLua;

public class BirthNpc
{
    public int Id;
    public string Name;
    public string ModelPath;
    public float Scale;
    public GameObject prefab;
    public int type;
    public int pid;
}
public class BirthMgr : MonoBehaviour
{
#if UNITY_EDITOR
    static List<BirthNpc> _npcs;
    static List<BirthNpc> _items;
    public static Dictionary<int, BirthNpc> NpcDic;
    //type 0=npc,1=item;
    public static List<BirthNpc> GetList(int type)
    {
        if (_npcs == null)
        {
            UpdateNpcInfo();
        }

        return type == 0 ? _npcs : _items;
    }
    public static int GetIndex(int type, int id)
    {
        return GetList(type).FindIndex(x => x.Id == id);
    }
    public void ReLoad()
    {
        LuaEnv luaenv = new LuaEnv();
        string str = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Product/map_npc.lua");
        object[] obs = luaenv.DoStringUTF8(str);
        LuaTable lua = obs[0] as LuaTable;
        LuaTable objs = null;
        lua.Get<string, LuaTable>("objects", out objs);
        //LuaTable lua = luaenv.LoadString<LuaTable>(str);
        objs.ForEach<int, LuaTable>(delegate (int id, LuaTable arr)
        {
            GameObject go = new GameObject(id.ToString());
            go.transform.SetParent(this.transform);
            BirthCamp camp = go.AddComp<BirthCamp>();
            string strNum = arr.Get<string, string>("GroupNum");
            camp.isLoop = strNum == "100000000";
            if (!camp.isLoop)
                camp.GroupNum = int.Parse(strNum);
            else
                camp.GroupNum = 0;
            camp.GroupDelay = arr.Get<string, float>("GroupDelay");
            //camp.EachDelay = arr.Get<string, float>("EachDelay");
            camp.type = 0;
            camp.Limit = 2;
            Vector3 pos = StringToVector3(arr, "RootPos");
            camp.transform.position = pos;
            LuaTable lt = arr.Get<string, LuaTable>("Objects");
            lt.ForEach<int, LuaTable>(delegate (int id2, LuaTable arr2)
            {
                int oId = arr2.Get<string, int>("Id");
                GameObject go2 = new GameObject(oId.ToString());
                BirthNode node = go2.AddComp<BirthNode>();
                node.Id = oId;
                go2.transform.SetParent(go.transform);
                Vector3 lPos = StringToVector3(arr2, "Pos");
                Vector3 dir = StringToVector3(arr2, "Dir");
                go2.transform.localPosition = lPos;
                go2.transform.eulerAngles = dir;
            });
        });
    }

    public static void UpdateNpcInfo()
    {
        NpcDic = new Dictionary<int, BirthNpc>();
        _npcs = new List<BirthNpc>();
        LuaEnv luaenv = new LuaEnv();
        string str = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Product/Lua/config/objects.lua");
        LuaTable lua = luaenv.DoStringUTF8(str)[0] as LuaTable;
        //LuaTable lua = luaenv.LoadString<LuaTable>(str);
        lua.ForEach<int, LuaTable>(delegate (int id, LuaTable arr)
        {
            BirthNpc npc = new BirthNpc();
            npc.Id = id;
            npc.Name = arr.Get<string, string>("Name");
            npc.ModelPath = arr.Get<string, string>("ModelPath");
            npc.type = 0;
            if (arr.ContainsKey<string>("Scale"))
                npc.Scale = arr.Get<string, float>("Scale");
            else
                npc.Scale = 1f;
            npc.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BundleResources/" + npc.ModelPath) as GameObject;
            _npcs.Add(npc);
            NpcDic[id] = npc;
        });
        _items = new List<BirthNpc>();
        str = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Product/Lua/config/items.lua");
        lua = luaenv.DoStringUTF8(str)[0] as LuaTable;
        lua.ForEach<int, LuaTable>(delegate (int id, LuaTable arr)
        {
            BirthNpc npc = new BirthNpc();
            npc.Id = id;
            npc.Name = arr.Get<string, string>("Name");
            npc.ModelPath = arr.Get<string, string>("File");
            npc.type = 1;
            npc.pid = arr.Get<string, int>("pid");
            if (arr.ContainsKey<string>("Size"))
                npc.Scale = arr.Get<string, float>("Size");
            else
                npc.Scale = 1f;
            npc.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BundleResources/" + npc.ModelPath) as GameObject;
            _items.Add(npc);
            NpcDic[id] = npc;
        });
        _items.Sort(delegate (BirthNpc npc1, BirthNpc npc2) { return npc1.pid - npc2.pid; });
        luaenv.Dispose();
    }

    private List<Vector3> posList;
    public int cellSize = 10;
    public int cellNum = 1;
    //public NavMesh mNavMesh;
    public float maxDistance = 10;
    public LayerMask areaMask = int.MaxValue;
    public Color genColor = Color.black;
    public void AutoGenPosList()
    {
        int size = 800;
        int chunkXY = size / cellSize;
        int count = cellNum * chunkXY* chunkXY;
       // posList = new Vector3[count];
        NavMeshHit hit;
        int index = 0;
        if (posList == null)
            posList = new List<Vector3>();
        posList.Clear();
        //Random.InitState(Time.renderedFrameCount);
        for (int x=0;x< size; x+= cellSize)
        {
            for (int z = 0; z < size; z += cellSize)
            {
                Vector3 startPos = new Vector3(x, 0, z);
                Vector3 endPos = new Vector3(x + cellSize, 0, z + cellSize);
                for (int num = 0; num < cellNum; num++)
                {
                    float xx = Random.Range(startPos.x, endPos.x);
                    float zz = Random.Range(startPos.z, endPos.z);
                    bool b = NavMesh.SamplePosition(new Vector3(xx, 0, zz), out hit, maxDistance, int.MaxValue);
                    if(hit.hit)
                    {
                        posList.Add(hit.position);
                        index++;
                    }
                    if (hit.hit != b)
                        Debug.Log(hit.position);
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("return {");
        int pcount = posList.Count;
        for (int i = 0; i < pcount; i++)
        {
            sb.AppendLine(Vector3ToString(posList[i], false) + ",");
        }
        sb.AppendLine("}");
        Debug.Log("Gen OK:" + count);
        string dir = Application.dataPath.Replace("/msgame_client/fiveProject/Assets", "/msgame_design/luaconfig/map/");
        File.WriteAllText(dir + "map_point.lua", sb.ToString());
        System.Diagnostics.Process.Start(dir);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying || posList == null)
            return;
        if (genColor.a <= 0f)
            return;
        Gizmos.color = genColor;
        int pcount = posList.Count;
        for (int i = 0; i < pcount; i++)
        {
            Vector3 pos = posList[i] + Vector3.up * 0.2f;
            //Gizmos.DrawIcon(this.transform.position, this.Id.ToString());
            Gizmos.DrawWireCube(pos, Vector3.one*0.3f);
        }
    }
    /*
   local max = 100000000;
   return {
       [1] = {
           GroupNum = max,
           GroupDelay = 3,
           EachDelay = 0, 
           RootPos = {50,50},
           Objects = {
               [1] = {Id = 1006, Pos = {0,0}, Dir = {0,0,0},},
               [2] = {Id = 1006, Pos = {1,0}, Dir = {0,0,0},},
               [3] = {Id = 1006, Pos = {0,1}, Dir = {0,0,0},},
               [4] = {Id = 1006, Pos = {1,1}, Dir = {0,0,0},},
           },
       },
   }
   */
    public static void PrintToLua()
    {
        List<NodeControl> godArmList = new List<NodeControl>();
        List<BirthCamp> objList = new List<BirthCamp>();
        List<BirthCamp> itemList = new List<BirthCamp>();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--代码生成文件;");
        sb.AppendLine("--GroupNum 群组数");
        sb.AppendLine("--GroupDelay 群组刷新延迟");
        sb.AppendLine("--EachDelay 每个怪刷新延迟");
        sb.AppendLine("--RootPos 群组根坐标");
        sb.AppendLine("--Objects 怪物列表");
       // sb.AppendLine("--Type, 0 == npc, 1 == item");

        sb.AppendLine("local max = 100000000;");
        sb.AppendLine("return {");
        GameObject[] goArr = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects();
        for (int g = 0; g < goArr.Length; g++)
        {
            BirthCamp[] camps = goArr[g].GetComponentsInChildren<BirthCamp>();
            for (int i = 0; i < camps.Length; i++)
            {
                if (camps[i].type == 0)
                    objList.Add(camps[i]);
                else if (camps[i].type == 1)
                    itemList.Add(camps[i]);
            }
            NodeControl[] nodeCtrls = goArr[g].GetComponentsInChildren<NodeControl>();
            for (int i = 0; i < nodeCtrls.Length; i++)
            {
                MapItemMono mono = nodeCtrls[i].GetComponent<MapItemMono>();
                if (mono != null && mono.Type == 9)
                {
                    godArmList.Add(nodeCtrls[i]);
                }
            }
        }
        
        sb.AppendLine("objects = {");
        AddBirthCamp(objList, sb);
        sb.AppendLine("},");
        
        sb.AppendLine("items = {");
        AddBirthCamp(itemList, sb, true);
        sb.AppendLine("},");

        
        sb.AppendLine("}");
        string dir = Application.dataPath.Replace("/msgame_client/fiveProject/Assets", "/msgame_design/luaconfig/map/");
        string path = dir + "map_npc.lua";
        File.WriteAllText(path, sb.ToString());


        StringBuilder godsb = new StringBuilder();
        godsb.AppendLine("--代码生成文件;");
        godsb.AppendLine("return {");
        AddGodArms(godArmList, godsb);
        godsb.AppendLine("}");
        File.WriteAllText(dir + "map_godarms.lua", godsb.ToString());

        string luaDir = Application.dataPath.Replace("/fiveProject/Assets", "/fiveProject/Product/Lua/config/client/");
        File.Copy(dir + "map_godarms.lua", luaDir + "map_godarms.lua", true);
        Debug.Log("导出地图出生点完成. " + path);

        System.Diagnostics.Process.Start(dir);
    }
    static void AddBirthCamp(List<BirthCamp> camps, StringBuilder sb,bool isItem = false)
    {
        for (int i = 0; i < camps.Count; i++)
        {
            BirthCamp camp = camps[i];
            sb.AppendLine("\t[" + (i + 1) + "] = {");
            int gId = EventTrigger3D.GetMapId(camp.gameObject);
            if (gId < 0)
                gId = i + 1;
            sb.AppendLine("\tGroupId = " + gId + ",");
            if (camp.isLoop)
                sb.AppendLine("\tGroupNum = max,");
            else
                sb.AppendLine("\tGroupNum = " + camp.GroupNum + ",");

            //if (isItem)
                sb.AppendLine("\tLimit = " + camp.Limit + ",");
            sb.AppendLine("\tGroupDelay = " + camp.GroupDelay + ",");
           // sb.AppendLine("\tEachDelay = " + camp.EachDelay + ",");
            sb.AppendLine("\tRootPos = " + Vector3ToString(camp.transform.position) + ",");
            BirthNode[] nodes = camp.gameObject.GetComponentsInChildren<BirthNode>();
            if(isItem)
            {
                sb.AppendLine("\tObjects = {");
                int idx = 0;
                for (int j = 0; j < nodes.Length; j++)
                {
                    BirthNode node = nodes[j];
                    if (node.Weight == 0)
                        continue;
                    idx++;
                    sb.Append("\t\t\t[" + idx + "] = {");
                    sb.AppendFormat("Id = {0}, Weight = {1}", node.Id, node.Weight);
                    sb.Append("},\n");
                }
                sb.AppendLine("\t\t},");
                idx = 0;
                sb.AppendLine("\tPositions = {");
                for (int j = 0; j < nodes.Length; j++)
                {
                    BirthNode node = nodes[j];
                    idx++;
                    sb.Append("\t\t\t[" + idx + "] = {");
                    sb.AppendFormat("Pos = {0}, Dir = {1},", Vector3ToString(node.transform.position - camp.transform.position), Vector3ToString(node.transform.eulerAngles));
                    sb.Append("},\n");
                }
                sb.AppendLine("\t\t},");
            }
            else
            {
                sb.AppendLine("\tObjects = {");
                int idx = 0;
                for (int j = 0; j < nodes.Length; j++)
                {
                    BirthNode node = nodes[j];
                    if (node.Weight == 0)
                        continue;
                    idx++;
                    sb.Append("\t\t\t[" + idx + "] = {");
                    sb.AppendFormat("Id = {0}, Pos = {1}, Dir = {2},", node.Id, Vector3ToString(node.transform.localPosition), Vector3ToString(node.transform.eulerAngles));
                    sb.Append("},\n");
                }
                sb.AppendLine("\t\t},");
            }

            sb.AppendLine("\t},");
        }
    }
    static void AddGodArms(List<NodeControl> godArmLists, StringBuilder sb, bool isRandom = false)
    {

        for (int i = 0; i < godArmLists.Count; i++)
        {
            NodeControl ctrl = godArmLists[i];
            ctrl.Awake();
            sb.AppendLine("[" + (i + 12101) + "] = {");
            int gId = EventTrigger3D.GetMapId(ctrl.gameObject);
            if (gId < 0)
                gId = i + 1;
            sb.AppendLine("\tmapId = " + gId + ",");
            sb.AppendLine("\tpos = " + Vector3ToString(ctrl.transform.position, false) + ",");
            sb.AppendLine("\ttargetPoint = " + Vector3ToString(ctrl.Get("targetPoint").position, false) + ",");
            sb.AppendLine("\tTreasury = {");
            for (int j=1;j<=6;j++)
            {
                Vector3 pos = ctrl.Get("fuhuobaoxiang0" + j).position;
                sb.AppendLine("\t\t[" + j + "] = " + Vector3ToString(pos, false) + ",");
            }
            sb.AppendLine("\t},");

            sb.AppendLine("\tBase = {");
            for (int j = 1; j <= 6; j++)
            {
                Vector3 pos = ctrl.Get("fuhuodiannao0" + j).position;
                sb.AppendLine("\t\t[" + j + "] = " + Vector3ToString(pos, false) + ",");
            }
            sb.AppendLine("\t},");

            sb.AppendLine("},");
        }
    }
    static string Vector3ToString(Vector3 vec, bool y = true)
    {
        if (y)
            return "{" + vec.x + "," + vec.z + "," + vec.y + "}";
        return "{" + vec.x + "," + vec.z + "}";
    }

    static Vector3 StringToVector3(LuaTable lua,string name,bool y = true)
    {
        Vector3 pos = Vector3.zero;
        LuaTable posLua = lua.Get<string, LuaTable>(name);
        pos.x = posLua.Get<int, float>(1);
        pos.z = posLua.Get<int, float>(2);
        if(y)
            pos.y = posLua.Get<int, float>(3);
        return pos;
    }
    // Update is called once per frame
    //private void OnDrawGizmos()
    //{
    //    BirthNode[] births = this.gameObject.GetComponentsInChildren<BirthNode>();
    //    List<BirthNpc> npcs = Npcs;
    //    for (int i=0;i<births.Length;i++)
    //    {
    //        Gizmos.color = Color.blue;
    //        if(births[i].transform.childCount == 0)
    //        {
    //            BirthNpc npc = null;
    //            NpcDic.TryGetValue(births[i].Id, out npc);
    //            if (npc != null && npc.prefab != null)
    //            {
    //                GameObject go = GameObject.Instantiate(npc.prefab, births[i].transform);
    //                go.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
    //                go.transform.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
    //                continue;
    //            }
    //        }
    //        Gizmos.DrawCube(births[i].transform.position + Vector3.up * 0.5f, Vector3.one);
    //    }
    //}
#endif
}
