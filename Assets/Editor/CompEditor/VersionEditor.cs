using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Frame;

//自定义Tset脚本
[CustomEditor(typeof(VersionStyle))]
//在编辑模式下执行脚本，这里用处不大可以删除。
[ExecuteInEditMode]
//请继承Editor
public class VersionEditor : Editor
{
    VersionStyle mScript;
    //public List<SDKStyle> mlist;
    Vector2 sdkView = Vector2.zero;
    void OnEnable()
    {
        //mlist = EditorPath.GetSDKStyleList();
    }
    //在这里方法中就可以绘制面板。
    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("open缓存", GUILayout.Height(35)))
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }
        if (GUILayout.Button("删除缓存",GUILayout.Height(35)))
        {
            VersionManager.DeleteLocalCache();
          //  MPrefs.SetString(Frame.Const.LanguageKey, "");
        }
        //if (GUILayout.Button("Refresh", GUILayout.Height(35)))
        //{
        //    mlist = EditorPath.GetSDKStyleList();
        //}
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
        //得到Test对象
        mScript = (VersionStyle)target;

        //mScript.GetUrl = EditorGUILayout.EnumPopup("GetUrl", mScript.GetUrl);
        //if (GUILayout.Button("打包"))
        //{
        //    BuildAPK.BuildAndroidPlayer(mScript.IsUpdateVersion, mScript.Version, mScript.Channel, mScript.Publish);
        //}

        //if (GUILayout.Button("打包正式"))
        //{

        //}
        
    }
    //public static void SetFBStyle(SDKStyle style)
    //{
    //    if (style.Channel != eChannel.Google)
    //        return;
    //    FaceBookSeting
    //}
}