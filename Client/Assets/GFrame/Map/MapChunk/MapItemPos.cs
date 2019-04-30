using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class MapItemPos : IItemPos
{
    public ushort id;
    public byte type = 0;//
    public Vector3 pos = Vector3.zero;
    public Vector3 euler = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public float size { get {
            float _size = Mathf.Abs(Mathf.Max(scale.x, scale.z));
            return _size;
        }
    }
    public LightMapData[] lightMapDataList = null;
    public uint getPosId(Vector2 cell)
    {
        return highlight.VectorTools.calculatePageIDByPos(this.pos, cell);
    }
    public MapItemPos clone()
    {
        MapItemPos info = new MapItemPos();
        info.id = this.id;
        info.pos = this.pos;
        info.euler = this.euler;
        info.scale = this.scale;
        info.type = this.type;
        return info;
    }
    public bool Equal(MapItemPos mp)
    {
        return mp.pos == this.pos && mp.scale == this.scale && mp.euler == this.euler && mp.id == this.id && mp.type == this.type;
    }
    public bool IsEdge()
    {
        return type >= 10 && type <= 20 && id != 16 && id != 46;
    }
    public static bool IsSurfaceWater(byte surface)
    {
        return surface > 0 && surface <= 13;
    }
    public byte CheckSurface(byte surface)
    {
        if (IsSurfaceWater(surface) && !IsEdge())
            surface = 0;
        return surface;
    }
}
[System.Serializable]
public class LightMapData
{
    public int lightmapIndex = -1;
    public Vector4 lightmapScaleOffset;
}

/*
 * 
 * 
 LightMapData curData = lightMapDataList;
            try
            {
                while (curData != null)
                {
                    LightMapNode node = curData.node;
                    Transform curTransform = mono.transform;
                    while (node != null)
                    {
                        curTransform = mono.transform.GetChild(node.index);
                        node = node.next;
                    }
                    
                    if (mr != null)
                    {
                        mr.lightmapIndex = curData.lightmapIndex;
                        mr.lightmapScaleOffset = curData.lightmapScaleOffset;
                    }
                    curData = curData.next;
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }

 **/
