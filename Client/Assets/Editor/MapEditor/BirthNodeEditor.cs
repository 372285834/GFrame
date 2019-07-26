using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using XLua;
using System.IO;
using System;

[CanEditMultipleObjects, CustomEditor(typeof(BirthNode))]
[ExecuteInEditMode]
public class BirthNodeEditor : Editor
{

    BirthNode mScript;

    static string splicFlag = "  ";
    public List<string> popList = new List<string>();
    static string searchStr = "";
    public override void OnInspectorGUI()
    {
        mScript = target as BirthNode;
        List<BirthNpc> list = mScript.dataList;
        if (list == null)
            return;
        // mScript.GroupNum = EditorGUILayout.IntField("群数量", mScript.GroupNum);
        // mScript.Id = EditorGUILayout.Popup()
        //type 0==npc,1==item;
        bool isItem = mScript.type == 1;
        if(isItem)
            mScript.Weight = EditorGUILayout.IntSlider("权重", mScript.Weight, 0, 1000);
        if(mScript.Weight > 0 || !isItem)
        {
            searchStr = EditorGUILayout.TextField("search:", searchStr);
            popList.Clear();
            int oldIdx = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (!string.IsNullOrEmpty(searchStr) && !list[i].Id.ToString().Contains(searchStr))
                    continue;
                popList.Add(list[i].Id + splicFlag + list[i].Name);
                if (list[i].Id == mScript.Id)
                    oldIdx = popList.Count - 1;
            }
            if (popList.Count == 0)
                return;
            int selectId = EditorGUILayout.Popup("id", oldIdx, popList.ToArray());
            if (selectId < 0 || selectId >= list.Count)
                return;
            string[] str = popList[selectId].Split(new string[] { splicFlag }, StringSplitOptions.RemoveEmptyEntries);
            int curId = 0;
           // Debug.Log(str[0]);
            if(Int32.TryParse(str[0], out curId))
            {
                if (curId != mScript.Id)
                {
                    mScript.Id = curId;
                    mScript.UpdateNpc();
                }
            }
        }
    }
}