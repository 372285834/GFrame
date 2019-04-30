using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEngine.UI;

[CanEditMultipleObjects,CustomEditor(typeof(BrokenItem))]
[ExecuteInEditMode]
//请继承Editor
public class BreakenItemEditor : Editor
{
    BrokenItem mScript;
    
    //在这里方法中就可以绘制面板。
    public override void OnInspectorGUI()
    {
        //serializedObject.Update();
        base.OnInspectorGUI();
        if (!Application.isPlaying)
            return;
        //得到Test对象
        mScript = (BrokenItem)target;
        if(!mScript.IsPlaying)
        {
            if (GUILayout.Button("Play"))
            {
                mScript.Play();
            }
        }
        else
        {
            if (GUILayout.Button("Stop"))
            {
                mScript.Stop();
            }
        }
        if (GUILayout.Button("Reset"))
            mScript.Reset();
        //if (GUILayout.Button("打包"))
        //{
        //    BuildAPK.BuildAndroidPlayer(mScript.IsUpdateVersion, mScript.Version, mScript.Channel, mScript.Publish);
        //}
        //if (GUILayout.Button("打包正式"))
        //{

            //}

    }
}