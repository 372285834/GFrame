using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MapItemMgr : MonoBehaviour
{
    public MapDataStyle mStyle;
    public Light mLight;
    public Collider mCollider;
    public Transform root;
    public bool drowLine = true;
    public HashSet<GameObject> grassMotionHash = new HashSet<GameObject>();
    public static MapItemMgr Inst;
    private void Awake()
    {
        Inst = this;
        MapDataStyle.CurMap = mStyle;
        mStyle.root = this.root;
    }
    void OnDestroy()
    {
        mStyle.Clear();
    }
    public void ShowShadow(int t)
    {
        mLight.shadows = (LightShadows)t;
    }
    public void Update()
    {
        mStyle.Update();
        UpdateGrassMotion();
    }
    public void AddGrassMotion(GameObject go)
    {
        if(go != null)
            this.grassMotionHash.Add(go);
    }
    public void RemoveGrassMotion(GameObject go)
    {
        if (go != null)
            this.grassMotionHash.Remove(go);
    }
    public HashSet<MapItemMono> nearItemList = new HashSet<MapItemMono>();
    public void UpdateGrassMotion()
    {
        //if (mStyle == null)
        //    return;
        //nearItemList.Clear();
        //if (mStyle.target != null)
        //    grassMotionHash.Add(mStyle.target.gameObject);
        //foreach (GameObject go in this.grassMotionHash)
        //{
        //    if (go == null)
        //        continue;
        //    mStyle.GetNearItembyChunk(0, nearItemList, 4, go.transform.position);
        //}
        //foreach (MapItemMono item in nearItemList)
        //{

        //    item.PlayMotion(grassMotionHash);
        //}
    }
    public LuaChunkPool luaPool;
   // [XLua.BlackList]
    public Color itemAOI = Color.magenta;
    private void OnDrawGizmos()
    {
        if (luaPool != null)
        {
            luaPool.DrawActiveChunk(itemAOI);
        }
        if (!drowLine)
            return;
#if UNITY_EDITOR
        mStyle.OnDrawGizmos();
#endif
    }
}
