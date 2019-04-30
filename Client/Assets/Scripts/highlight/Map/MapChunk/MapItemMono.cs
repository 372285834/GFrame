using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum eMapItemType
{
    Prefab = 0,
    Mesh = 1,
}
[System.Serializable]
public class MapItemPrefabData
{
    public int tempId;
    public string guid;
    public string abName;
    public MapItemMono prefab;
    public eMapItemType eType { get { return prefab.eType; } }
    public int size { get { return prefab.size; } }
    public bool callLua { get { return prefab.callLua; } }
    public int Type { get { return prefab.Type; } }
    public Material[] materials;
    public Mesh mesh;
    public Vector3[] vertices
    {
        get {
            if (mesh != null)
                return mesh.vertices;
            return null;
        }
    }
    public bool isPreLoad = true;
    public void Init()
    {
        if (eType == eMapItemType.Mesh)
        {
            MeshFilter mFilter = this.prefab.GetComponent<MeshFilter>();
            MeshRenderer mRender = this.prefab.GetComponent<MeshRenderer>();
            this.mesh = mFilter.sharedMesh;
            this.materials = mRender.sharedMaterials;
        }
        //BirthCamp bc = this.prefab.GetComponentInChildren<BirthCamp>(true);
        //if (bc != null)
        //    GameObject.DestroyImmediate(bc.gameObject);
    }
    public void Load()
    {
        if(prefab == null)
        {

        }
    }
}
#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
public class MapItemMono : MonoBehaviour
{
    public enum eStateType
    {
        Clear,
        Normal,
        PlayMotion,
    }
    public eMapItemType eType = eMapItemType.Prefab;
    public int size;
    public bool callLua = false;
    public int Type = 0;
    public int tempId { get { return data.tempId; } }
    public int mapId
    {
        get
        {
            Vector3 pos = this.transform.position;
            int id = Mathf.FloorToInt(pos.x) * 100000 + Mathf.FloorToInt(pos.z);
            return id;
        }
    }
    // private eStateType stateType = eStateType.Clear;

    public MapItemPrefabData data { get; private set; }
    [NonSerialized]
    public MeshFilter mFilter;
    [NonSerialized]
    public MeshRenderer mRender;
    [NonSerialized]
    public MapItemPos mPosData;
    public MeshRenderer[] mRenders;
    public float showSize { get; private set; }
   // private void Awake()
   // {
        // this.enabled = false;
   // }
    public void Init()
    {
        mFilter = this.gameObject.GetComponent<MeshFilter>();
        mRender = this.gameObject.GetComponent<MeshRenderer>();
    }
    public void SetData(MapItemPrefabData _data)
    {
        data = _data;
        this.eType = data.eType;
        this.size = data.size;
        this.Type = data.Type;
        this.callLua = data.callLua;
    }
    public void SetPosData(MapItemPos itemPos)
    {
        this.mPosData = itemPos;
        this.transform.position = itemPos.pos;
        this.transform.eulerAngles = itemPos.euler;
        this.transform.localScale = itemPos.scale;
    }
    public void SetMesh(Mesh mesh)
    {
        if (mFilter != null)
            mFilter.sharedMesh = mesh;
    }
    public void SetMaterial(Material[] mats)
    {
        if (mRender != null)
            mRender.sharedMaterials = mats;
    }

    public void DeSerializeLightMapData(MapItemPos posData)
    {
     //   stateType = eStateType.Normal;
        showSize = data.size * posData.size;
        if (posData.lightMapDataList == null || posData.lightMapDataList.Length == 0)
            return;
        try
        {
            if (this.mRenders.Length == posData.lightMapDataList.Length)
            {
                for (int i = 0; i < this.mRenders.Length; i++)
                {
                    if (this.mRenders[i] == null)
                        continue;
                    this.mRenders[i].lightmapIndex = posData.lightMapDataList[i].lightmapIndex;
                    this.mRenders[i].lightmapScaleOffset = posData.lightMapDataList[i].lightmapScaleOffset;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("lightmap error: " + e.Message);
        }
    }

    public static void SerializeLightMapData(MapItemMono mono,MapItemPos posData)
    {
        posData.lightMapDataList = null;
       // try
        {
            if (mono.mRenders.Length > 0)
            {
                posData.lightMapDataList = new LightMapData[mono.mRenders.Length];
                for (int i = 0; i < mono.mRenders.Length; i++)
                {
                    posData.lightMapDataList[i] = new LightMapData();
                    posData.lightMapDataList[i].lightmapIndex = mono.mRenders[i].lightmapIndex;
                    posData.lightMapDataList[i].lightmapScaleOffset = mono.mRenders[i].lightmapScaleOffset;
                }
            }

        }
        //catch (Exception e)
        //{
        //    Debug.LogError(e.Message + "\n" + e.StackTrace);
        //}
    }
    //private HashSet<GameObject> grassHash;
    //private void Update()
    //{
    //    if (grassHash == null)
    //        return;
    //    if (stateType == eStateType.PlayMotion)
    //    {
    //        Mesh mesh = mFilter.mesh;
    //        Vector3[] list = data.vertices;
    //        foreach (var go in grassHash)
    //        {
    //            Vector3 dir = this.transform.InverseTransformPoint(go.transform.position);
    //            dir = -dir.normalized;
    //            float dis = dir.magnitude;
    //            //float off = SRPSetting.Inst.motionPower / dis;
    //            // if (off > 1f)
    //            //    off = 1f;
    //            //Vector3 sPos = this.transform.InverseTransformPoint(targetPos);// targetPos - this.transform.position;
    //            for (int i = 0; i < mesh.vertexCount; i++)
    //            {
    //                Vector3 pos = list[i];
    //                // Vector3 dir = pos - sPos;
    //                float off = Mathf.Sin((float)(SRPSetting.Inst.motionSpeed * Time.realtimeSinceStartup * pos.y)) * SRPSetting.Inst.motionPower;// / dis
    //                pos.x += off * dir.x;// * pos.y * SRPSetting.Inst.grassDirScale;
    //                pos.z += off * dir.z;// * pos.y * SRPSetting.Inst.grassDirScale;
    //                list[i] = pos;
    //            }
    //        }
    //        mesh.vertices = list;
    //        //mesh.RecalculateNormals();
    //    }
    //}
    //public void PlayMotion(HashSet<GameObject> hash)
    //{
    //    if (data.mesh == null || !data.mesh.isReadable)
    //        return;
    //    if (stateType == eStateType.PlayMotion)
    //    {
    //        grassHash = hash;
    //    }
    //    else if (stateType == eStateType.Normal)
    //    {
    //        grassHash = hash;
    //        this.enabled = true;
    //        stateType = eStateType.PlayMotion;
    //    }
    //}
    private void OnDestroy()
    {
        Clear();
    }
    public void Clear()
    {
     //   this.enabled = false;
    //    grassHash = null;
        this.transform.position = new Vector3(0f, 10000f, 0f);
      //  this.id = -1;
      //  stateType = eStateType.Clear;
    }
    //#if UNITY_EDITOR
    //    //void Update()
    //    //{
    //    //    if (!IsLookAtCamera)
    //    //        return;
    //    //    ThreeLockCamera.SetLookAtCamera(this.transform);
    //    //}
    //    public void DrawGizmos()
    //    {
    //            Gizmos.color = new Color(0f, 1f, 0f, 1f);
    //            Vector3 off = new Vector3(0.5f, 0f, 0.5f) * (mPosData.size - 1);
    //            Vector3 size = new Vector3(1f, 0f, 1f) * mPosData.size;
    //            Gizmos.DrawWireCube(mPosData.pos + off, size);
    //    }
    //#endif
}
