using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Tile
{
    Floor,//地板
    Wall//墙
}
public class RandomMap : MonoBehaviour
{

    public int row = 30;
    public int col = 30;
    private Tile[,] mapArray;
    public GameObject wall, floor, player;
    private GameObject map;
    public Transform maps;
    private int forTimes = 0;//SmoothMapArray循环次数
    // Use this for initialization
    void Start()
    {
        mapArray = new Tile[row, col];
       // maps = GameObject.FindGameObjectWithTag("map").transform;
        //map = new GameObject();
        //map.transform.SetParent(maps);
        //CreateMap ();

        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            InitMap();
        }
        //下一步
        if (Input.GetKeyDown(KeyCode.E))
        {
            CreateMap();
        }
    }
    private void InitMapArray()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                //采用<50%生成墙
                mapArray[i, j] = Random.Range(0, 100) < 40 ? Tile.Wall : Tile.Floor;
                //边界置为墙
                if (i == 0 || j == 0 || i == row - 1 || j == col - 1)
                {
                    mapArray[i, j] = Tile.Wall;
                }
            }
        }
    }

    private Tile[,] SmoothMapArray0()
    {
        Tile[,] newMapArray = new Tile[row, col];
        int wallCount1 = 0, wallCount2 = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                wallCount1 = CheckNeighborWalls(mapArray, i, j, 1);
                wallCount2 = CheckNeighborWalls(mapArray, i, j, 2);
                if (mapArray[i, j] == Tile.Wall)
                {
                    newMapArray[i, j] = (wallCount1 >= 4) ? Tile.Wall : Tile.Floor;
                }
                else
                {
                    newMapArray[i, j] = (wallCount1 >= 5 || wallCount2 <= 2) ? Tile.Wall : Tile.Floor;
                }
                if (i == 0 || i == row - 1 || j == 0 || j == col - 1)
                {
                    newMapArray[i, j] = Tile.Wall;
                }
            }
        }
        return newMapArray;
    }

    //4-5规则 
    //当前墙：周围超过4个保持为墙
    //当前地板：周围超过5个墙变为墙
    //循环4-5次
    private Tile[,] SmoothMapArray1()
    {
        Tile[,] newMapArray = new Tile[row, col];
        int wallCount = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                wallCount = CheckNeighborWalls(mapArray, i, j, 1);
                if (mapArray[i, j] == Tile.Wall)
                {
                    newMapArray[i, j] = (wallCount >= 4) ? Tile.Wall : Tile.Floor;
                }
                else
                {
                    newMapArray[i, j] = (wallCount >= 5) ? Tile.Wall : Tile.Floor;
                }
                if (i == 0 || i == row - 1 || j == 0 || j == col - 1)
                {
                    newMapArray[i, j] = Tile.Wall;
                }
            }
        }
        return newMapArray;
    }

    //判断周围墙的数量
    private int CheckNeighborWalls(Tile[,] mapArray, int i, int j, int t)
    {
        int count = 0;
        for (int k = i - t; k <= i + t; k++)
        {
            for (int l = j - t; l <= j + t; l++)
            {
                if (k >= 0 && k < row && l >= 0 && l < col)
                {
                    if (mapArray[k, l] == Tile.Wall)
                    {
                        count++;
                    }
                }
            }
        }
        //去除本身是否为墙
        if (mapArray[i, j] == Tile.Wall)
        {
            count--;
        }
        return count;
    }

    private void InstanceMap()
    {
        bool setPlayer = true;
        if(map != null)
            Destroy(map);
        map = new GameObject();
        map.transform.SetParent(maps);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (mapArray[i, j] == Tile.Floor)
                {
                    GameObject go = Instantiate(floor, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(map.transform);
                    //设置层级
                //    go.layer = LayerMask.NameToLayer("floor");

                    if (setPlayer)
                    {
                        //设置角色
                        GameObject g_player = Instantiate(player, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                        g_player.transform.SetParent(map.transform);
                        setPlayer = false;
                    }
                }
                else if (mapArray[i, j] == Tile.Wall)
                {
                    GameObject go = Instantiate(wall, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(map.transform);
                   // go.layer = LayerMask.NameToLayer("wall");
                }
            }
        }
    }


    private void InitMap()
    {
        forTimes = 0;
        //Destroy(map);
        //map = new GameObject();
        //map.transform.SetParent(maps);
        InitMapArray();
        InstanceMap();
    }

    private void CreateMap()
    {
        //Destroy(map);
        //map = new GameObject();
        //map.transform.SetParent(maps);
        if (forTimes < 7)
        {
            if (forTimes < 4)
            {
                mapArray = SmoothMapArray0();
            }
            else
            {
                mapArray = SmoothMapArray1();
            }
            forTimes++;
        }
        InstanceMap();
    }

    private void GenerateMap()
    {
        forTimes = 0;
        //map = new GameObject();
        //map.transform.SetParent(maps);
        InitMapArray();
        while (forTimes < 7)
        {
            if (forTimes < 4)
            {
                mapArray = SmoothMapArray0();
            }
            else
            {
                mapArray = SmoothMapArray1();
            }
            forTimes++;
        }
        InstanceMap();
    }

}