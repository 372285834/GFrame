using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using highlight;

public struct ChunkSite
    {
        public static ChunkSite zero = new ChunkSite(0, 0);
        public ChunkSite(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.rect = Rect.zero;
        }
        public int x;
        public int y;
        public Rect rect;
        public Vector3 position { get { return rect.center.ToVector3(); } }
        public Vector3 min { get { return rect.min.ToVector3(); } }
        public Vector3 max { get { return rect.max.ToVector3(); } }
        public Vector3 size { get { return rect.size.ToVector3(); } }
        public uint id { get { return VectorTools.calculatePageID((ushort)this.x, (ushort)this.y); } }
        public static ChunkSite Creat(highlight.Vector2Int vec, Vector2 cell, Vector3 root)
        {
            return Creat(vec.x,vec.y,cell,root);
        }
        public static ChunkSite Creat(int x, int y, Vector2 cell, Vector3 root)
        {
            ChunkSite site;
            site.x = x;
            site.y = y;
            Vector3 min = VectorTools.GetPos(site.x, site.y, -0.5f, -0.5f, root, cell);
            site.rect = new Rect(min.ToV2(), cell);
            return site;
        }
        public static ChunkSite Creat(Vector3 pos, Vector2 cell, Vector3 root)
        {
            ChunkSite site;
        highlight.Vector2Int vec = VectorTools.GetXY(pos, root, cell);
            site.x = vec.x;
            site.y = vec.y;
            Vector3 min = VectorTools.GetPos(site.x, site.y, -0.5f, -0.5f, root, cell);
            site.rect = new Rect(min.ToV2(), cell);
            return site;
        }
    }
    public abstract class Chunk
    {
        public ChunkSite site;
        public IChunkPool mPool;
        public bool IsActive = false;
        public List<IItemPos> posList { get { return this.mPool.GetChunkData(site.id); } }
        public abstract void InitChunk();
        public virtual void Update() { }
        public virtual bool CheckRefresh() {
            return true;
        }
        public abstract void Clear();
        public void Init(IChunkPool pool)
        {
            //MUtil.BeginSample("InitChunk");
            this.mPool = pool;
            this.InitChunk();
            //MUtil.EndSample();
        }
    }
    public interface IChunkPool
    {
        Vector3 GetPos(int x, int y, float xf, float yf);
    highlight.Vector2Int GetXY(Vector3 pos);
        Vector2 Cell { get;}
        //bool isGrid { get; }
        List<IItemPos> GetChunkData(uint id);
        object getParam();
    }
public interface IItemPos
{
    uint getPosId(Vector2 cell);
}
[System.Serializable]
public class ChunkPoolData
{
    public Vector2 pos;
    public Vector2 cell = new Vector2(5f, 5f);
    public highlight.Vector2Int size;
    public highlight.Vector2Int off;
    public int radius;
    public Color color;
}
public class ChunkPool<T> : IChunkPool where T : Chunk, new()
{
    //public Dictionary<ulong, Chunk> mDic = new Dictionary<ulong, Chunk>();
    public Dictionary<uint, List<IItemPos>> mMap = new Dictionary<uint, List<IItemPos>>();
    public bool isGrid { get; private set; }
    // public bool IsGrid() { return isGrid; }
    public List<Chunk> mList = new List<Chunk>();
    public Dictionary<uint, Chunk> mActiveDic = new Dictionary<uint, Chunk>();
    public Chunk mCurChunk { get { return GetChunk(this.xId, this.yId); } }
    public Vector2 Cell { get; private set; }    //最小单元大小
    public int XNum;
    public int YNum;
    public int offsetX = 0;
    public int offsetY = 0;
    private object param;
    public object getParam() { return this.param; }
    public void setParam(object obj) { this.param = obj; }
    public int radius;
    private int allNum;
    public int xId { get { return mPos.x; } }
    public int yId { get { return mPos.y; } }
    private highlight.Vector2Int mPos;
    highlight.RectInt mRect = highlight.RectInt.zero;
    static ChunkSite defaultPos = new ChunkSite(-10000, -10000);
    private bool isFirstUpdate = true;
    public Rect mWorldRect;
    public highlight.RectInt overRect;
    public bool IsCheckRefresh = false;
    public Vector3 min { get; private set; }
    public Vector3 center { get { return mWorldRect.center.ToVector3(); } }
    public Vector3 max { get { return mWorldRect.max.ToVector3(); } }
    public Vector3 size { get { return mWorldRect.size.ToVector3(); } }

    public virtual void Init(ChunkPoolData data, bool _isGrid = false)
    {
        Init(data.pos, data.cell, data.size.x, data.size.y, data.radius, data.off.x, data.off.y, _isGrid);
    }
    public virtual void Init(Vector2 _position, Vector2 _cell, int _sizeX, int _sizeY, int _r, int _offX, int _offY, bool _isGrid = false)
    {
        if (_r < 1)
        {
            Debug.LogError("error: radius < 1");
            return;
        }
        this.Cell = _cell;
        XNum = _sizeX;
        YNum = _sizeY;
        this.radius = _r;
        this.offsetX = _offX;
        this.offsetY = _offY;
        allNum = (2 * _r + 1) * (2 * _r + 1);
        mUpdateDistance = Mathf.Min(Cell.x, Cell.y)*0.5f;
        mMap.Clear();
        ClearItem();
        isGrid = _isGrid;
        isFirstUpdate = true;
        mPos = new highlight.Vector2Int(-1, -1);
        this.param = null;
        min = new Vector3(_position.x, 0f, _position.y);
        mWorldRect = new Rect(_position, new Vector2(_cell.x * _sizeX, _cell.y * _sizeY));
    }
    public void SetMapData(List<IItemPos> list)
    {
        mMap.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            AddMapData(list[i]);
        }
        this.isGrid = false;
    }
    public void AddMapData(IItemPos pos)
    {
        uint id = pos.getPosId(this.Cell);
        List<IItemPos> pList = GetChunkData(id);
        if (pList == null)
        {
            pList = new List<IItemPos>();
            mMap[id] = pList;
        }
        pList.Add(pos);
    }
    public void RemoveMapData(IItemPos pos)
    {
        uint id = pos.getPosId(this.Cell);
        List<IItemPos> pList = GetChunkData(id);
        if (pList != null)
        {
            pList.Remove(pos);
        }
    }
    public List<IItemPos> GetChunkData(Vector3 pos)
    {
        uint id = VectorTools.calculatePageIDByPos(pos, this.Cell);
        return GetChunkData(id);
    }
    public List<IItemPos> GetChunkData(uint id)
    {
        List<IItemPos> pList = null;
        mMap.TryGetValue(id, out pList);
        return pList;
    }
    private Vector3 mPosition = Vector3.zero;
    private Vector3 lastPostion = Vector3.zero;
    public Vector3 focusPos { get { return GetPos(this.xId, this.yId); } }
    public Vector3 focusCenterPos { get { return this.GetPos(xId - offsetX, yId - offsetY); } }
    public float mUpdateDistance = 1f;
    public virtual bool Check(Vector3 pos)
    {
        mPosition = pos;
        highlight.Vector2Int curPos = GetXY(mPosition);
        if (mPos == curPos)
            return false;
        mPos = curPos;
        //float dis = Mathf.Abs(Vector3.Distance(mPosition, lastPostion));
        //if (!isFirstUpdate && dis < mUpdateDistance)
        //    return false;
        lastPostion = mPosition;
        UpdateState();
        return true;
    }
    static List<Chunk> tempList = new List<Chunk>();
    static List<highlight.Vector2Int> plist = new List<highlight.Vector2Int>();
    public void UpdateState(bool force = false)
    {
        if (!isGrid && (mMap == null || mMap.Count == 0))
            return;
        force = force || isFirstUpdate;
        highlight.RectInt newRect = new highlight.RectInt(xId - offsetX, yId - offsetY, radius, new highlight.Vector2Int(this.XNum - 1, this.YNum - 1));
        this.overRect = highlight.RectInt.Overlap(mRect, newRect);
        plist.Clear();
        tempList.Clear();
        if (overRect.IsInverse())
            force = true;
        for (int cy = newRect.up; cy <= newRect.down; cy++)
        {
            for (int cx = newRect.left; cx <= newRect.right; cx++)
            {
                if (!force)
                {
                    if (overRect.IsInside(cx, cy))
                        continue;
                }
                plist.Add(new highlight.Vector2Int(cx, cy));
            }
        }

        if (overRect.IsInverse())
        {
            ClearItem();
        }
        for (int i = mList.Count; i < allNum; i++)
        {
            Chunk chunk = new T() as Chunk;
            chunk.mPool = this;
            chunk.site = defaultPos;
            mList.Add(chunk);
        }
        int num = 0;
        int pCount = plist.Count;
        mActiveDic.Clear();
        //Profiler.BeginSample("UpdateItem_" + pCount);
        try
        {
            //MUtil.BeginSample("ForChunkList");
            for (int i = 0; i < allNum; i++)
            {
                Chunk ck = mList[i];
                if (!force)
                {
                    ChunkSite pos = ck.site;
                    if (overRect.IsInside(pos.x, pos.y))
                    {
                        this.AddActiveChunk(ck, true, false);
                        continue;
                    }
                }
                ck.IsActive = false;
                ck.Clear();
                if (num >= pCount)
                {
                    ck.site = defaultPos;
                }
                else
                {
                    ck.site = ChunkSite.Creat(plist[num], this.Cell, this.min);
                    this.AddActiveChunk(ck, false, force);
                }
                num++;
            }
            //MUtil.EndSample();
            for (int i = 0; i < tempList.Count; i++)
            {
                // MUtil.BeginSample("Chunk.Init");
                tempList[i].Init(this);
                // MUtil.EndSample();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
        plist.Clear();
        tempList.Clear();
        isFirstUpdate = false;
        mRect = newRect;
    }
    void AddActiveChunk(Chunk ck, bool isInRect, bool force)
    {
        mActiveDic.Add(ck.site.id, ck);
        if (!IsCheckRefresh || force)
        {
            ck.IsActive = true;
            if (!isInRect)
                tempList.Add(ck);
            return;
        }
        bool isChange = ck.CheckRefresh();
        if (isChange)
        {
            if (ck.IsActive)
            {
                tempList.Add(ck);
            }
            else
            {
                //ck.site = defaultPos;
                ck.Clear();
            }
        }
    }
    public Dictionary<uint, Chunk>.Enumerator GetChunks()
    {
        return mActiveDic.GetEnumerator();
    }
    public Chunk GetChunk(int x, int y)
    {
        if (x < 0 || y < 0 || x >= XNum || y >= YNum)
            return null;
        Chunk ck = null;
        uint id = VectorTools.calculatePageID((ushort)x, (ushort)y);
        mActiveDic.TryGetValue(id, out ck);//.Find(item => item.site.x == x && item.site.y == y);
        if (ck == null)
            return null;
        return ck;
    }
    public Chunk GetChunk(Vector3 pos)
    {
        highlight.Vector2Int point = GetXY(pos);
        return GetChunk(point.x, point.y);
    }
    public void GetNearbyChunk(List<Chunk> list,Vector3 pos)
    {
        highlight.Vector2Int point = GetXY(pos);
        for(int x=-1;x<=1;x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Chunk ck = GetChunk(point.x + x, point.y + y);
                if (ck != null)
                    list.Add(ck);
            }
        }
    }
    public void UpdateActive()
    {
        var dictEnumerator = mActiveDic.GetEnumerator();
        while (dictEnumerator.MoveNext())
        {
            Chunk ck = dictEnumerator.Current.Value;
            UpdateChunk(ck);
        }
    }
    public void UpdateChunk(int x, int y)
    {
        UpdateChunk(GetChunk(x, y));
    }
    public void UpdateChunk(Chunk ck)
    {
        if (ck == null)
            return;
        ck.Update();
    }
    public bool IsInctiveChunkPool(int x, int y)
    {
        bool b = mRect.IsInside(x, y);
        return b;
    }
    public Vector3 GetActiveRectSize()
    {
        return new Vector3(mRect.width * Cell.x, 0f, mRect.height * Cell.y);
    }
    public highlight.Vector2Int GetXY(Vector3 pos)
    {
        highlight.Vector2Int point = VectorTools.GetXY(pos, this.min, this.Cell);
        point.x = Mathf.Clamp(point.x, 0, this.XNum - 1);
        point.y = Mathf.Clamp(point.y, 0, this.YNum - 1);
        return point;
    }
    public Vector3 GetPos(int x, int y)
    {
        return GetPos(x, y, 0f, 0f);
    }
    public Vector3 GetPos(int x, int y, float offx, float offy)
    {
        return VectorTools.GetPos(x, y, offx, offy, this.min, this.Cell);
    }
    public Vector3 ClampPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, mWorldRect.min.x, mWorldRect.max.x);
        pos.z = Mathf.Clamp(pos.z, mWorldRect.min.y, mWorldRect.max.y);
        return pos;
    }
    public bool Contains(Vector3 pos)
    {
        return mWorldRect.Contains(pos);
    }
    public virtual void Clear()
    {
        ClearItem();
        mMap.Clear();
        mList.Clear();
        mActiveDic.Clear();
        mRect = highlight.RectInt.zero;
        mPos = new highlight.Vector2Int(-1, -1);
        lastPostion = Vector3.zero;
    }
    public void ClearItem()
    {
        //lastPostion = Vector2.zero;
        var dictEnumerator = mActiveDic.GetEnumerator();
        while (dictEnumerator.MoveNext())
        {
            Chunk ck = dictEnumerator.Current.Value;
            ck.Clear();
        }
        mActiveDic.Clear();
        mRect = highlight.RectInt.zero;
        mPos = new highlight.Vector2Int(-1, -1);
        lastPostion = Vector3.zero;
    }

    public void DrawLine(Color co)
    {
        VectorTools.DrawLine(co, min, Cell, XNum, YNum);
    }
    public void DrawActiveChunk(Color co)
    {
        Gizmos.color = co;
        var itor = this.GetChunks();
        while (itor.MoveNext())
        {
            Chunk wck = itor.Current.Value as Chunk;
            Gizmos.DrawWireCube(wck.site.position, wck.site.size);
        }
        Gizmos.DrawWireCube(this.focusPos, GetActiveRectSize());
    }
}
