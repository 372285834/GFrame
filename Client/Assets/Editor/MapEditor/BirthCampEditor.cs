using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
[CanEditMultipleObjects, CustomEditor(typeof(BirthCamp))]
[ExecuteInEditMode]
public class BirthCampEditor : Editor
{
    public static BirthCamp curSelectNode;
    public bool isSelect = false;
    BirthCamp mScript;
    public string[] typeArr = new string[] { "Object", "Item" };
    BirthNode[] GetNodes()
    {
        return mScript.gameObject.GetComponentsInChildren<BirthNode>();
    }
    public override void OnInspectorGUI()
    {
        isSelect = curSelectNode == mScript;
        isSelect = EditorGUILayout.Toggle("显示Cube", isSelect);
        if (isSelect)
            curSelectNode = mScript;
        mScript = target as BirthCamp;
        mScript.transform.localRotation = Quaternion.identity;
        mScript.transform.localScale = Vector3.one;
        mScript.isLoop = EditorGUILayout.Toggle("循环",mScript.isLoop);
        if (!mScript.isLoop)
            mScript.GroupNum = EditorGUILayout.IntField("总群数",mScript.GroupNum);
        mScript.GroupDelay = EditorGUILayout.FloatField("每群刷新延迟",mScript.GroupDelay);
        //  mScript.EachDelay = EditorGUILayout.FloatField("每个刷新延迟", mScript.EachDelay);
        mScript.Limit = EditorGUILayout.IntField("最大随机数量", mScript.Limit);

        int curType = EditorGUILayout.Popup("type", mScript.type, typeArr);
        bool changeType = mScript.type == curType;
        mScript.type = curType;

        GUILayout.Label("每群 " + mScript.transform.childCount + " 个");
        if (GUILayout.Button("Add"))
        {
            GameObject go = new GameObject("node" + mScript.transform.childCount);
            go.AddComponent<BirthNode>();
            go.transform.SetParent(mScript.transform);
            Selection.activeObject = go;
        }
        BirthNode[] births = GetNodes();
        if (GUILayout.Button("刷新显示出生点") || changeType)
        {
            BirthMgr.UpdateNpcInfo();
            for (int i = 0; i < births.Length; i++)
            {
                births[i].UpdateNpc();
            }
        }
        if (GUILayout.Button("Clear显示"))
        {
            BirthMgr.UpdateNpcInfo();
            for (int i = 0; i < births.Length; i++)
            {
                births[i].ClearNpc();
            }
        }
        for (int i = 0; i < births.Length; i++)
        {
            births[i].showSelfGizmos = isSelect;
        }
    }
}