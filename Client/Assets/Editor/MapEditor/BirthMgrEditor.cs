using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CanEditMultipleObjects, CustomEditor(typeof(BirthMgr))]
[ExecuteInEditMode]
public class BirthMgrEditor : Editor
{
    BirthMgr mScript;
    BirthNode[] GetNodes()
    {
            return mScript.gameObject.GetComponentsInChildren<BirthNode>();
    }
    public override void OnInspectorGUI()
    {
        mScript = target as BirthMgr;
        mScript.transform.position = Vector3.zero;
        mScript.transform.localRotation = Quaternion.identity;
        mScript.transform.localScale = Vector3.one;
        if (GUILayout.Button("刷新显示出生点"))
        {
            BirthMgr.UpdateNpcInfo();
            BirthNode[] births = GetNodes();
            for (int i = 0; i < births.Length; i++)
            {
                births[i].UpdateNpc();
            }
        }
        if (GUILayout.Button("Clear显示"))
        {
            BirthMgr.UpdateNpcInfo();
            BirthNode[] births = GetNodes();
            for (int i = 0; i < births.Length; i++)
            {
                births[i].ClearNpc();
            }
        }
        
        if (GUILayout.Button("自动生成点"))
        {
            mScript.AutoGenPosList();
        }
        if(GUILayout.Button("导出种怪种道具数据"))
        {
            BirthMgr.PrintToLua();
        }
        if(GUILayout.Button("恢复数据"))
        {
            mScript.ReLoad();
        }
        base.OnInspectorGUI();
            //if(GUILayout.Button("导出lua"))
            //{
            //    mScript.PrintToLua();
            //}
        }
}