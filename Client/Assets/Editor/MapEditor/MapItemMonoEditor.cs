using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CanEditMultipleObjects, CustomEditor(typeof(MapItemMono))]
[ExecuteInEditMode]
public class MapItemMonoEditor : Editor
{
    MapItemMono mScript;
    public override void OnInspectorGUI()
    {
        mScript = target as MapItemMono;
        EditorGUILayout.LabelField("mapId:  " + mScript.mapId);
        if(mScript.data != null)
        {
            EditorGUILayout.ObjectField("tempId:" + mScript.data.tempId, mScript.data.prefab, typeof(MapItemMono), true);
        }
        base.OnInspectorGUI();
            
        MapItemMono[] monos = mScript.GetComponentsInChildren<MapItemMono>();
        if(monos.Length != 1)
        {
            Debug.LogError("禁止MapItemMono预设嵌套:" + mScript.name);
        }
    }
}