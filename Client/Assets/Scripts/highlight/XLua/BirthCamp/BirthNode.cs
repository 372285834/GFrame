using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BirthNode : MonoBehaviour
{
    public int Id;
    [Range(0,1000)]
    public int Weight = 1;
    //public GameObject prefab;
#if UNITY_EDITOR
    private BirthCamp mCamp;
    public BirthCamp Camp
    {
        get
        {
            if (mCamp == null)
                mCamp = this.transform.parent.GetComponent<BirthCamp>();
            return mCamp;
        }
    }
    //type 0=npc,1=item;
    public int type { get { return Camp.type; } }
    public List<BirthNpc> dataList { get { return BirthMgr.GetList(this.type); } }
    public BirthNpc NpcData
    {
        get
        {
            BirthNpc npc = null;
            BirthMgr.NpcDic.TryGetValue(Id, out npc);
            return npc;
        }
    }
    public void UpdateNpc()
    {
        ClearNpc();
        if (NpcData != null && Weight > 0)
        {
            if (NpcData.prefab != null)
            {
                GameObject go = GameObject.Instantiate(NpcData.prefab, this.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * NpcData.Scale;
                go.hideFlags = HideFlags.HideAndDontSave;// HideFlags.DontSave | HideFlags.NotEditable;
            }
        }
    }
    public void ClearNpc()
    {
        while (this.transform.childCount > 0)
            GameObject.DestroyImmediate(this.transform.GetChild(0).gameObject);
    }
    public bool IsError
    {
        get
        {
            UnityEngine.AI.NavMeshHit hit;
            Vector3 pos = this.transform.position;
            bool b = UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 0.2f, int.MaxValue);
            return b;
        }
    }
    public static bool showGizmos = false;
    public bool showSelfGizmos = false;
    private void OnDrawGizmos()
    {
        if(!showGizmos)
        {
            if (!showSelfGizmos)
                return;
        }
        Gizmos.color = IsError ? Color.blue : Color.red;
        Gizmos.DrawCube(this.transform.position + Vector3.up * 0.5f*0.2f, Vector3.one*0.2f);
       //    Handles.color = IsError ? Color.blue : Color.red;
        //   Handles.DrawWireCube(this.transform.position + Vector3.up * 0.5f, Vector3.one);
        //Gizmos.DrawIcon(this.transform.position, this.Id.ToString());
       // Handles.ConeHandleCap(Id, this.transform.position + Vector3.up * 0.5f, Quaternion.identity, 1, EventType.ContextClick);
     //    Handles.Button(this.transform.position + Vector3.up * 0.5f, Quaternion.identity, 1, 1, CapFunction);
    }
    void CapFunction(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
    {
        Selection.activeObject = this.gameObject;
    }
#endif
}
