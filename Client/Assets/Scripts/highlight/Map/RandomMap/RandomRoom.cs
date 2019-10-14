using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRoom : MonoBehaviour
{

    //地图大小

    public int height = 128;
    public int width = 128;

    //房间大小
    public int roomMaxSize = 5;

    //预制引用
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject roomPrefab;
    public GameObject opendoorPrefab;
    public GameObject closedoorPrefab;

    //生成房间次数
    public int createTimes = 100;

    //当前区域数量
    private int currentRegionNum = -1;

    //记录区域数组
    private int[,] regions;

    //记录房间数组
    private List<Room> rooms;

    //记录边缘数组
    private List<Vector2> bounds;

    //地图数据
    private int[,] mapArray;

    //tile类型
    private enum tileType { floor, wall, room, opendoor, closedoor };

    //4个基本方向
    private Vector2[] baseDirection = { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) };

    //迷宫父物体
    private GameObject mazeParent;

    //生成迷宫
    public void generate()
    {
        creatBoard(mapArray);

        for (int i = 0; i < createTimes; i++)
        {
            addRooms();
        }

        growMaze(new Vector2(1, 1));

        connectRegions();

        removeDeadEnds();

        drawMaze();

        //Debug.Log(1);
    }

    //增加房间
    public void addRooms()
    {
        Room room = CreateRoom();

        bool overLap = false;

        if (room.X2 >= width || room.Y2 >= width)
        {
            overLap = true;
        }

        if (room.X1 >= width || room.Y1 >= width)
        {
            overLap = true;
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            if (room.inAnotherRoom(rooms[i]))
            {
                overLap = true;
                break;
            }
        }

        if (!overLap)
        {
            rooms.Add(room);
            currentRegionNum++;
            fillRoom(room);
        }
    }

    //初始化迷宫
    public void creatBoard(int[,] map)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = (int)tileType.wall;
                if (i == -1 || j == -1 || i == width || j == height)
                {
                    bounds.Add(new Vector2(i, j));
                    //map[i, j] = 0;
                }
            }
        }
    }

    //生成过道
    public void growMaze(Vector2 start)
    {
        List<Vector2> cells = new List<Vector2>();
        List<Vector2> unmadeCells = new List<Vector2>();
        Vector2 cell;

        _startRegion();
        _carve(start, tileType.floor);
        cells.Add(start);

        while (cells.Count > 0)
        {
            cell = cells[cells.Count - 1];
            unmadeCells = new List<Vector2>();
            foreach (Vector2 dir in baseDirection)
            {
                if (_canCarve(cell, dir))
                    unmadeCells.Add(dir);
            }

            if (unmadeCells.Count > 0)
            {
                Vector2 dir;

                //Debug.Log(unmadeCells[0]);
                dir = unmadeCells[Random.Range(0, unmadeCells.Count)];
                _carve(cell + dir, tileType.floor);
                _carve(cell + dir * 2, tileType.floor);
                cells.Add(cell + dir * 2);
            }
            else
            {
                cells.RemoveAt(cells.Count - 1);
            }
        }
    }

    bool _canCarve(Vector2 pos, Vector2 direction)
    {
        // Must end in bounds.
        if (bounds.Contains(pos + direction * 3))
            return false;

        // Destination must not be open.
        Vector2 temppos = pos + direction * 2;

        if (temppos.x > 0 && temppos.x < width - 1 && temppos.y > 0 & temppos.y < height - 1)
        {
            return mapArray[(int)temppos.x, (int)temppos.y] == (int)tileType.wall;
        }
        else
        {
            return false;
        }
    }

    void _carve(Vector2 pos, tileType type)
    {
        mapArray[(int)pos.x, (int)pos.y] = (int)type;
        regions[(int)pos.x, (int)pos.y] = currentRegionNum;
    }

    void _startRegion()
    {
        currentRegionNum++;
    }

    //连接区域
    public void connectRegions()
    {

        //找出连接多个区域的连接点
        List<Vector2>[] connectors = new List<Vector2>[currentRegionNum];

        for (int k = 1; k < currentRegionNum; k++)
        {
            connectors[k] = new List<Vector2>();
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    if (roundWallCount(i, j) > 2)
                        continue;

                    if (mapArray[i, j] != (int)tileType.wall)
                        continue;

                    if (regions[i, j] == k)
                    {
                        if (regions[i + 1, j] != regions[i, j] && mapArray[i + 1, j] != (int)tileType.wall)
                        {
                            connectors[k].Add(new Vector2(i, j));
                            continue;
                        }

                        if (regions[i - 1, j] != regions[i, j] && mapArray[i - 1, j] != (int)tileType.wall)
                        {
                            connectors[k].Add(new Vector2(i, j));
                            continue;
                        }

                        if (regions[i, j + 1] != regions[i, j] && mapArray[i, j + 1] != (int)tileType.wall)
                        {
                            connectors[k].Add(new Vector2(i, j));
                            continue;
                        }

                        if (regions[i, j - 1] != regions[i, j] && mapArray[i, j - 1] != (int)tileType.wall)
                        {
                            connectors[k].Add(new Vector2(i, j));
                            continue;
                        }
                    }
                }
            }

            Vector2 temppos = new Vector2();

            if (connectors[k].Count > 0)
            {
                temppos = connectors[k][Random.Range(0, connectors[k].Count - 1)];
                mapArray[(int)temppos.x, (int)temppos.y] = (int)tileType.closedoor;
            }
        }
    }

    //删除死胡同
    public void removeDeadEnds()
    {
        //查找所有路的集合
        List<Vector2> floors = new List<Vector2>();

        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (mapArray[i, j] == (int)tileType.floor)
                {
                    floors.Add(new Vector2(i, j));
                }
            }
        }

        while (true)
        {
            bool noDeadLoad = true;

            foreach (Vector2 floor in floors)
            {
                if (roundWallCount((int)floor.x, (int)floor.y) > 2)
                {
                    floors.Remove(floor);
                    mapArray[(int)floor.x, (int)floor.y] = (int)tileType.wall;
                    noDeadLoad = false;
                    break;
                }
            }

            if (noDeadLoad)
                break;
        }
    }

    //查找死胡同
    public void findDeadLoad(List<Vector2> floors)
    {

    }

    //计算周边墙的数量
    public int roundWallCount(int i, int j)
    {
        int roundwallcount = 0;

        if (mapArray[i + 1, j] == (int)tileType.wall)
            roundwallcount++;

        if (mapArray[i - 1, j] == (int)tileType.wall)
            roundwallcount++;

        if (mapArray[i, j + 1] == (int)tileType.wall)
            roundwallcount++;

        if (mapArray[i, j - 1] == (int)tileType.wall)
            roundwallcount++;

        return roundwallcount;
    }

    //增加连接点
    public void addJunction()
    {

    }

    //查找连接多区域的连接点
    public void findConnector()
    {

    }

    //填充房间
    public void fillRoom(Room room)
    {
        for (int i = room.X1; i < room.X2; i++)
        {
            for (int j = room.Y1; j < room.Y2; j++)
            {
                mapArray[i, j] = room.roomData[i - room.X1, j - room.Y1];
                regions[i, j] = currentRegionNum;
                bounds.Add(new Vector2(i, j));
            }
        }
    }

    //创建随机大小房间
    public Room CreateRoom()
    {
        int roomHeight = Random.Range(2, roomMaxSize) + 2;
        int roomWidth = Random.Range(2, roomMaxSize) + 2;
        int x = Random.Range(1, width / 2) * 2;
        int y = Random.Range(1, height / 2) * 2;
        Room room = new Room(x, y, x + roomWidth, y + roomHeight);

        return room;
    }

    //绘制迷宫
    public void drawMaze()
    {
        mazeParent = new GameObject();
        mazeParent.name = "Maze";

        for (int i = 0; i < width; i++)
        {
            //string log = "";
            for (int j = 0; j < height; j++)
            {
                //log += mapArray[i, j];
                if (mapArray[i, j] == (int)tileType.floor)
                {
                    GameObject go = Instantiate(floorPrefab, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(mazeParent.transform);
                }
                else if (mapArray[i, j] == (int)tileType.wall)
                {
                    GameObject go = Instantiate(wallPrefab, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(mazeParent.transform);
                }
                else if (mapArray[i, j] == (int)tileType.room)
                {
                    GameObject go = Instantiate(roomPrefab, new Vector3(i, -0.5f, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(mazeParent.transform);
                }
                else if (mapArray[i, j] == (int)tileType.opendoor)
                {
                    GameObject go = Instantiate(opendoorPrefab, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(mazeParent.transform);
                }
                else if (mapArray[i, j] == (int)tileType.closedoor)
                {
                    GameObject go = Instantiate(closedoorPrefab, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
                    go.transform.SetParent(mazeParent.transform);
                }
            }
            //Debug.Log(log);
        }
    }
    // Use this for initialization
    void Start()
    {
        mapArray = new int[height, width];
        mazeParent = new GameObject();
        mazeParent.name = "Maze";
        rooms = new List<Room>();
        bounds = new List<Vector2>();
        regions = new int[height, width];

        // creatBoard(mapArray);
        //drawMaze();

        generate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReCreateMap();
        }
    }

    private void ReCreateMap()
    {
        mapArray = new int[height, width];
        bounds = new List<Vector2>();
        regions = new int[height, width];
        GameObject.Destroy(mazeParent);
        Destroy(GameObject.Find("Maze"));
        mazeParent = new GameObject();
        mazeParent.name = "Maze";
        rooms = new List<Room>();
        generate();
    }

    //计算周围墙的数量
    //参数t为计算几圈
    int CheckNeighborWalls(int[,] array, int i, int j, int t)
    {
        int count = 0;

        for (int i2 = i - t; i2 < i + t + 1; i2++)
        {
            for (int j2 = j - t; j2 < j + t + 1; j2++)
            {
                if (i2 > 0 && i2 < width && j2 >= 0 && j2 < height)
                {
                    if (array[i2, j2] == (int)tileType.wall)
                    {
                        count++;
                    }
                }
            }
        }

        if (array[i, j] == (int)tileType.wall)
            count--;//因为上面的循环包含了自身，属于要将自身减掉

        return count;
    }

    /*
    public void OnDrawGizmos()
    {
        for (int i = 0; i < width; i++)
        {
            //string log = "";
            for (int j = 0; j < height; j++)
            {
                //log += mapArray[i, j];
                Color co = Color.white;
                Vector3 pos = new Vector3(i,0,j);
                if (mapArray[i, j] == (int)tileType.floor)
                {
                    pos.y = -0.5f;
                }
                else if (mapArray[i, j] == (int)tileType.wall)
                {
                    pos.y = 0.5f;
                    co = Color.black;
                }
                else if (mapArray[i, j] == (int)tileType.room)
                {
                    pos.y = -0.5f;
                    co = Color.blue;
                }
                else if (mapArray[i, j] == (int)tileType.opendoor)
                {
                   // pos.y = -0.5f;
                    co = Color.cyan;
                }
                else if (mapArray[i, j] == (int)tileType.closedoor)
                {
                    //pos.y = -0.5f;
                    co = Color.red;
                }
                Gizmos.color = co;
                Gizmos.DrawCube(pos, Vector3.one);
            }
            //Debug.Log(log);
        }
    }*/
}






public class Room
{
    private enum tileType { floor, wall, room };
    private enum RoomType { empty, bedroom, classroom };
    public int X1, Y1, X2, Y2;

    public int roomType = (int)RoomType.empty;

    public int[,] roomData;

    public Room(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;

        roomData = new int[X2 - X1, Y2 - Y1];
        CreateRoomData(roomType);
    }

    public void CreateRoomData(int roomType)
    {
        switch (roomType)
        {
            case (int)RoomType.empty:
                for (int i = 0; i < X2 - X1; i++)
                {
                    for (int j = 0; j < Y2 - Y1; j++)
                    {
                        if (i == 0 || i == X2 - X1 - 1 || j == 0 || j == Y2 - Y1 - 1)
                        {
                            roomData[i, j] = (int)tileType.wall;
                        }
                        else
                        {
                            roomData[i, j] = (int)tileType.room;
                        }
                        //roomData[i, j] = (int)tileType.room;
                    }
                }
                break;
            default:
                for (int i = 0; i < X2 - X1; i++)
                {
                    for (int j = 0; j < Y2 - Y1; j++)
                    {
                        if (i == 0 || i == X2 - X1 - 1 || j == 0 || j == Y2 - Y1 - 1)
                        {
                            roomData[i, j] = (int)tileType.wall;
                        }
                        else
                        {
                            roomData[i, j] = (int)tileType.room;
                        }
                        //roomData[i, j] = (int)tileType.room;
                    }
                }
                break;
        }

    }

    public bool outOfMap(int[,] map)
    {
        return false;
    }

    public bool inAnotherRoom(Room other)
    {
        if (X1 >= other.X1 && X1 <= other.X2 && Y1 >= other.Y1 && Y1 <= other.Y2)
        {
            return true;
        }
        if (X2 <= other.X2 && X2 >= other.X1 && Y2 <= other.Y2 && Y2 >= other.Y1)
        {
            return true;
        }
        if (X1 >= other.X1 && X1 <= other.X2 && Y2 >= other.Y1 && Y2 <= other.Y2)
        {
            return true;
        }
        if (X2 <= other.X2 && X2 >= other.X1 && Y1 <= other.Y2 && Y1 >= other.Y1)
        {
            return true;
        }

        return false;
    }
}