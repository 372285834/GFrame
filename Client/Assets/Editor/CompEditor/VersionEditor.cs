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
    public List<ChannelData> mlist;
    public string[] names;
    Vector2 sdkView = Vector2.zero;
    void OnEnable()
    {
        mlist = EditorPath.GetChannelList();
        names = new string[mlist.Count];
        for (int i = 0; i < mlist.Count; i++)
        {
            names[i] = mlist[i].Channel.ToString();
        }
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
            //MPrefs.SetString(Frame.Const.LanguageKey, "");
        }
        //if (GUILayout.Button("Refresh", GUILayout.Height(35)))
        //{
        //    mlist = EditorPath.GetSDKStyleList();
        //}
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
        //得到Test对象
        mScript = (VersionStyle)target;
        DrawChannel();
        //mScript.GetUrl = EditorGUILayout.EnumPopup("GetUrl", mScript.GetUrl);
        //if (GUILayout.Button("打包"))
        //{
        //    BuildAPK.BuildAndroidPlayer(mScript.IsUpdateVersion, mScript.Version, mScript.Channel, mScript.Publish);
        //}

        //if (GUILayout.Button("打包正式"))
        //{

        //}

    }
    void DrawChannel()
    {
        if (mlist != null && mlist.Count > 0)
        {
            GUILayout.Space(30);
            int curIdx = mlist.FindIndex(x => x == mScript.channelData);
            if (curIdx < 0)
                curIdx = 0;
            curIdx = EditorGUILayout.Popup(curIdx, names);
            mScript.channelData = mlist[curIdx];
            //sdkView = GUILayout.BeginScrollView(sdkView, false, true, GUILayout.Height(mlist.Count * 27f));
            //for (int i = 0; i < mlist.Count; i++)
            //{
            //    ChannelData data = mlist[i];
            //    GUILayout.BeginHorizontal();
            //    string tagStr = data.Channel + " " + data.bundleDisplayName + " " + data.bundleName;
            //    if (GUILayout.Button(tagStr, GUILayout.Height(25)))
            //    {
            //        Selection.activeObject = data;
            //    }
            //    bool isUse = data == mScript.channelData;
            //    GUI.enabled = !isUse;
            //    if (GUILayout.Button("Use", GUILayout.Height(25)))
            //    {
            //        mScript.channelData = data;
            //        //SetFBStyle(sdk);
            //        EditorUtility.SetDirty(mScript);
            //    }
            //    GUI.enabled = true;
            //    GUILayout.EndHorizontal();
            //}
            if (mScript.channelData == null)
                mScript.channelData = mlist[0];
        }
    }
    public static void SetChannel(eChannel c)
    {
        List<ChannelData> mlist = EditorPath.GetChannelList();
        for (int i = 0; i < mlist.Count; i++)
        {
            if(mlist[i].Channel == c)
            {
                VersionStyle.Instance.channelData = mlist[i];
                return;
            }
        }
    }
    //public static void SetFBStyle(SDKStyle style)
    //{
    //    if (style.Channel != eChannel.Google)
    //        return;
    //    FaceBookSeting
    //}
}