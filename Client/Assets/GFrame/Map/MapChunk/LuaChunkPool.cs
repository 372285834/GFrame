using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaItemPos : IItemPos
{
    public int Id;
    public Vector3 pos;
    public uint getPosId(Vector2 cell)
    {
        return highlight.VectorTools.calculatePageIDByPos(this.pos, cell);
    }
}
public class LuaItem : Chunk
{
    public LuaItem()
    {

    }
    public override void InitChunk()
    {
        List<IItemPos> list = this.posList;
        if (list == null)
            return;
        //Clear();
        Action<int> InitAction = (this.mPool as LuaChunkPool).InitAction;
        if (InitAction != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                LuaItemPos itemPos = list[i] as LuaItemPos;
                InitAction(itemPos.Id);
            }
        }
            
    }
    public override void Clear()
    {
        List<IItemPos> list = this.posList;
        if (list == null)
            return;
        Action<int> ClearAction = (this.mPool as LuaChunkPool).ClearAction;
        if (ClearAction != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                LuaItemPos itemPos = list[i] as LuaItemPos;
                ClearAction(itemPos.Id);
            }
        }
    }
}
public class LuaChunkPool : ChunkPool<LuaItem>
{
    public static LuaChunkPool Get()
    {
        return new LuaChunkPool();
    }
 //   [XLua.LuaCallCSharp]
 //   [XLua.CSharpCallLua]
    public Action<int> InitAction;
 //   [XLua.LuaCallCSharp]
 //   [XLua.CSharpCallLua]
    public Action<int> ClearAction;
    public Dictionary<int, LuaItemPos> posDic = new Dictionary<int, LuaItemPos>();
    public override void Init(Vector2 _position, Vector2 _cell, int _sizeX, int _sizeY, int _r, int _offX, int _offY, bool _isGrid = false)
    {
        posDic.Clear();
        base.Init(_position, _cell, _sizeX, _sizeY, _r, _offX, _offY, _isGrid);
    }
    public override bool Check(Vector3 pos)
    {
        return base.Check(pos);
    }
    public bool AddItem(int id, Vector3 pos)
    {
        LuaItemPos data = new LuaItemPos();
        data.Id = id;
        data.pos = pos;
        this.AddMapData(data);
        posDic[id] = data;
        if (this.GetChunk(pos) != null)
        {
            return true;
        }
        return false;
    }
    public bool RemoveItem(int id)
    {
        LuaItemPos data = null;
        posDic.TryGetValue(id, out data);
        if (data != null)
        {
            this.RemoveMapData(data);
            return true;
        }
        return false;
    }
    public override void Clear()
    {
        base.Clear();
        this.posDic.Clear();
    }
}
