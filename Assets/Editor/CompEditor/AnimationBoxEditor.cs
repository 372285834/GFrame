using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

[CustomEditor(typeof(highlight.AnimationBox))]
[ExecuteInEditMode]
public class AnimationBoxEditor : Editor
{
    highlight.AnimationBox mScript;
    float value = 1f;
    bool isProgressBar = false;
    List<string> clipList = new List<string>();
    int selectIdx = 0;
    static string errorClip = "Take 001";
    public override void OnInspectorGUI()
    {
        //serializedObject.Update();
        base.OnInspectorGUI();
        //得到Test对象
        mScript = (highlight.AnimationBox)target;
        if (mScript.mAnimation == null)
            mScript.mAnimation = mScript.GetComponent<Animation>();
        Animation ani = mScript.mAnimation;
        if (ani == null)
            return;
        clipList.Clear();
        try
        {
            foreach (AnimationState _state in ani)
            {
                if (_state.name == errorClip)
                    continue;
                clipList.Add(_state.name);
            }
        }
        catch(Exception e)
        {

        }
        clipList.Sort();
        if (clipList.Count > 0)
        {
            selectIdx = EditorGUILayout.Popup("播放选择", selectIdx, clipList.ToArray());
            AnimationClip clip = ani.GetClip(clipList[selectIdx]);
            if (clip != null)
            {
                if (GUILayout.Button("Play", GUILayout.Width(150f)))
                {
                    ani.Play(clipList[selectIdx]);
                }
                string path = AssetDatabase.GetAssetPath(clip);
                GUILayout.Label("Loop:" + clip.isLooping);
                GUILayout.Label("Length:" + clip.length);
                GUILayout.Label("Path:" + path);
                /*
                if(path.EndsWith(".fbx") || path.EndsWith(".FBX"))
                {
                    if (GUILayout.Button("解析优化Clip"))
                    {
                        string toPath = path.Remove(path.Length - 4, 4) + "/";
                        PrefabUtil.ParseFbx(path,false);

                        if (ani.clip != null && ani.clip.name == errorClip)
                        {
                            ani.clip = ani.GetClip("wait");
                        }
                        AnimationClip errclip = ani.GetClip(errorClip);
                        if (errclip != null)
                            ani.RemoveClip(errclip);
                        if (string.IsNullOrEmpty(toPath))
                            return;

                        for (int i = 0; i < clipList.Count; i++)
                        {
                            AnimationClip cp = ani.GetClip(clipList[i]);
                            string cName = cp.name;
                            string toName = toPath + cName + ".anim";
                            AnimationClip newClip = AssetDatabase.LoadMainAssetAtPath(toName) as AnimationClip;
                            if (cp != null)
                                ani.RemoveClip(cp);
                            if (newClip != null)
                            {
                                ani.AddClip(newClip, cName);
                                Debug.Log("addClip:" + toName);
                            }
                        }
                        if (ani.clip!= null)
                        {
                            ani.clip = ani.GetClip(ani.clip.name);
                        }
                    }
                }
                */
            }

            
        }
    }
}