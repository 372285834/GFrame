using UnityEngine;
using System.Collections;
using UnityEditor;
using Frame;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class ApplyTool
{
    [InitializeOnLoadMethod]
    static void StartInitializeOnLoadMethod()
    {
        PrefabUtility.prefabInstanceUpdated = prefabInstanceUpdated;
    }
    
    static bool IsApplying = false;
    static void prefabInstanceUpdated(GameObject instance)
    {
        if (Application.isPlaying)
            return;
        if (IsApplying)
            return;
        IsApplying = true;
        try
        {
            UnityEngine.GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            //BirthMgr mgr = prefab.GetComponent<BirthMgr>();
            //if (mgr != null)
            //{
                
            //}
        }
        catch(Exception e)
        {						
			Debug.LogError(e.Message + "\n" + e.StackTrace);
        }
        IsApplying = false;
    }
}
