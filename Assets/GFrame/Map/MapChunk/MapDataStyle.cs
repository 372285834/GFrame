using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataStyle : ScriptableObject
{
    public class ItemChunk : Chunk
    {
        public List<MapItemMono> itemList = new List<MapItemMono>();
        public ItemChunk()
        {

        }
        public override void InitChunk()
        {
            List<IItemPos> list = this.posList;
           // Clear();
            if (list == null)
                return;
            for (int i = 0; i < list.Count; i++)
            {
                MapItemPos itemPos = list[i] as MapItemPos;
                MapItemMono mono = CurMap.GetItem(itemPos.id);
                itemList.Add(mono);
                mono.SetPosData(itemPos);
                if(mono.data.eType == eMapItemType.Mesh)
                {
                    mono.SetMesh(mono.data.mesh);
                    mono.SetMaterial(mono.data.materials);
                    if (mono.gameObject.layer != mono.data.prefab.gameObject.layer)
                        mono.gameObject.layer = mono.data.prefab.gameObject.layer;
                }
                mono.DeSerializeLightMapData(itemPos);
                if (mono.data.callLua)
                {
                    if (CurMap.InitAction != null)
                        CurMap.InitAction(mono);
                }
            }
        }
        public void GetItemByType(HashSet<MapItemMono> list,int type)
        {
            int iCount = itemList.Count;
            for (int i = 0; i < iCount; i++)
            {
                if (itemList[i].data.Type == type)
                    list.Add(itemList[i]);
            }
        }
        public override void Clear()
        {
            int iCount = itemList.Count;
            for (int i = 0; i < iCount; i++)
            {
                if (itemList[i].data.callLua)
                {
                    if (CurMap.ClearAction != null)
                        CurMap.ClearAction(itemList[i]);
                }
                CurMap.ReleaseItem(itemList[i]);
            }
            itemList.Clear();
        }
    }
    //public string sceneUrl;
    public GameObject Temp;
    public bool IsAutoDestory = true;
    public bool IsAutoDeActive = true;
    //public float chunkFactor = 1.2f;
    public List<ChunkPoolData> ChunkPoolDataList = new List<ChunkPoolData>();
    public List<ChunkPool<ItemChunk>> ChunkPoolList = new List<ChunkPool<ItemChunk>>();
    public List<GameObject> preLoadList = new List<GameObject>();

    public List<MapItemPos> dataList = new List<MapItemPos>();
    public List<MapItemPrefabData> mapItemPrefabDataList = new List<MapItemPrefabData>();
    public Dictionary<int, MapItemPrefabData> mapItemPrefabDataDic = new Dictionary<int, MapItemPrefabData>();
    
    public Dictionary<int, GameObjectPool<MapItemMono>> mPrefabDic = new Dictionary<int, GameObjectPool<MapItemMono>>();
    private Stack<MapItemMono> itemPool = new Stack<MapItemMono>();
    public Transform target;
    public Transform root;
    public static MapDataStyle CurMap;
  //  [XLua.LuaCallCSharp]
   // [XLua.CSharpCallLua]
    public Action<MapItemMono> InitAction;
   // [XLua.LuaCallCSharp]
 //   [XLua.CSharpCallLua]
    public Action<MapItemMono> ClearAction;
    public void Init()
    {
        int poolLength = ChunkPoolDataList.Count;
        for (int i=0;i< poolLength; i++)
        {
            ChunkPool<ItemChunk> pool = new ChunkPool<ItemChunk>();
            pool.Init(ChunkPoolDataList[i]);
            ChunkPoolList.Add(pool);
        }
        for (int i = 0; i < mapItemPrefabDataList.Count; i++)
        {
            MapItemPrefabData data = mapItemPrefabDataList[i];
            if (data.prefab == null)
            {
                Debug.LogError("GetItem prefab == null pId:" + data.tempId);
                continue;
            }
            mapItemPrefabDataDic[data.tempId] = data;
            //if(data.eType == eMapItemType.Prefab && data.prefab.mRenders.Length == 0)
            //    data.prefab.mRenders = data.prefab.gameObject.GetComponentsInChildren<MeshRenderer>();
            data.Init();
            if (data.isPreLoad)
            {
                data.Load();
            }
            //if (data.eType == eMapItemType.Prefab)
        }
        int count = dataList.Count;
        for (int i = 0; i < count; i++)
        {
            MapItemPos itemPos = dataList[i];
            MapItemPrefabData data = GetPrefabData(itemPos.id);
            if(data != null)
            {
                int size = Mathf.FloorToInt(data.size * itemPos.size);
                for (int j = 0; j < poolLength; j++)
                {
                    if (size <= ChunkPoolList[j].Cell.x || j == poolLength - 1)
                    {
                        ChunkPoolList[j].AddMapData(itemPos);
                        break;
                    }
                }
            }
        }
    }
    public void Update()
    {
        if (target == null)
            return;
        for (int i = 0; i < ChunkPoolList.Count; i++)
        {
            ChunkPoolList[i].Check(target.position);
        }
    }
    public MapItemPrefabData GetPrefabData(int id)
    {
        MapItemPrefabData mono = null;
        mapItemPrefabDataDic.TryGetValue(id, out mono);
        return mono;
    }
    public void OnDrawGizmos()
    {
        for (int i=0;i< ChunkPoolList.Count;i++)
        {
            ChunkPool<ItemChunk> pool = ChunkPoolList[i];
            //pool.DrawLine();
            pool.DrawActiveChunk(ChunkPoolDataList[i].color);
        }
        
    }
    public static void GetAction(MapItemMono bm)
    {
    }
    public static void ReleaseAction(MapItemMono bm)
    {
        bm.Clear();
    }
    public MapItemMono GetItem(int pId)
    {
        return GetItem(this.Temp, this.root, pId);
    }
    public MapItemMono GetItem(GameObject temp, Transform parent, int pId)
    {
        MapItemPrefabData data = GetPrefabData(pId);
        if (data == null)
            return null;
        MapItemMono item = null;
        if (data.eType == eMapItemType.Prefab)
        {
            GameObjectPool<MapItemMono> pool = null;
            mPrefabDic.TryGetValue(pId, out pool);
            if (pool == null)
            {
                if(data.prefab == null)
                {
                    Debug.LogError("GetItem prefab == null pId:" + pId);
                    return null;
                }
                pool = new GameObjectPool<MapItemMono>(data.prefab.gameObject, GetAction, ReleaseAction, this.root);
                mPrefabDic[pId] = pool;
            }
            item = pool.Get(parent);
        }
        else
        {
            if (itemPool.Count == 0)
            {
                GameObject go = GameObject.Instantiate(temp, parent) as GameObject;
                //go.name = temp.name + itemNum;
                //itemNum++;
                item = go.GetComponent<MapItemMono>();
                if (item == null)
                    item = go.AddComponent<MapItemMono>();
                item.Init();
            }
            else
            {
                item = itemPool.Pop();
                //item.mMatBox.SetVisible(true);
            }
        }
        item.SetData(data);
        item.gameObject.SetActive(true);
        return item;
    }
    public void ReleaseItem(MapItemMono element)
    {
        if (element == null)
            return;
        if(IsAutoDestory)
        {
            GameObject.DestroyImmediate(element.gameObject);
            return;
        }
        if (element.data.eType == eMapItemType.Prefab)
        {
            int pid = (int)element.mPosData.id;
            if (mPrefabDic.ContainsKey(pid))
                mPrefabDic[pid].Release(element);
            else
            {
                GameObject.DestroyImmediate(element.gameObject);
                return;
            }
        }
        else
        {
            element.Clear();
            itemPool.Push(element);
            //element.mMatBox.SetVisible(false);
        }
        if (IsAutoDeActive)
            element.gameObject.SetActive(false);
    }

    public void GetNearItembyChunk(int idx, HashSet<MapItemMono> list, int type, Vector3 pos)
    {
        ChunkPool<ItemChunk> pool = this.ChunkPoolList[idx];
        highlight.Vector2Int point = pool.GetXY(pos);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                ItemChunk itemCk = pool.GetChunk(point.x + x, point.y + y) as ItemChunk;
                if (itemCk != null)
                {
                    itemCk.GetItemByType(list, type);
                }
            }
        }
    }
    public virtual void Clear()
    {
        for(int i=0;i<ChunkPoolList.Count;i++)
        {
            ChunkPoolList[i].Clear();
        }
        ChunkPoolList.Clear();
        mapItemPrefabDataDic.Clear();
        target = null;
        root = null;
        mPrefabDic.Clear();
        itemPool.Clear();
    }
}