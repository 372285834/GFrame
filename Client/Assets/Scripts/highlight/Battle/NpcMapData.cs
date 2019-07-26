using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcGroupData
{
    public int GroupId;
    public int GroupNum;
    public int Limit;
    public int GroupDelay;
    public Vector3 RootPos;
    public NpcMapData[] Npcs;
    public string ai;
}

public class NpcMapData
{
    public NpcGroupData Group;
    public int Id;
    public Vector3 Pos;
    public Vector3 Dir;
    public Vector3[] Path;

}
