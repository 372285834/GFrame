
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CanEditMultipleObjects, CustomEditor(typeof(MiniMapTool))]
[ExecuteInEditMode]
public class MiniMapToolEditor : Editor
{
    MiniMapTool mScript;
    public override void OnInspectorGUI()
    {
        mScript = target as MiniMapTool;
        mScript.transform.position = Vector3.zero;
        mScript.transform.localRotation = Quaternion.identity;
        mScript.transform.localScale = Vector3.one;
        base.OnInspectorGUI();
        if (GUILayout.Button("导出小地图快照"))
        {
            string path = Application.dataPath + "/Temp/miniMap_"+System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png";
            mScript.tex = mScript.mCamera.CaptureCamera(mScript.rect, path);
            AssetDatabase.Refresh();
            Selection.activeObject = mScript.tex;
        }
        //if(GUILayout.Button("导出lua"))
        //{
        //    mScript.PrintToLua();
        //}
    }
}