using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CanEditMultipleObjects, CustomEditor(typeof(RenderBox))]
[ExecuteInEditMode]
public class RenderBoxEditor : Editor
{
    RenderBox mScript;
    float alpha = 1f;
    bool testAlpha = false;
    public override void OnInspectorGUI()
    {
        mScript = target as RenderBox;
        base.OnInspectorGUI();
        if (GUILayout.Button("溶解"))
        {
            mScript.DissolveStart();
        }
        testAlpha = GUILayout.Toggle(testAlpha, "半透测试");
        if (testAlpha)
        {
            alpha = EditorGUILayout.Slider("半透", alpha, 0f, 1f);
            RenderBox.SetAlpha(mScript.gameObject, alpha);
        }
        //if(GUILayout.Button("导出lua"))
        //{
        //    mScript.PrintToLua();
        //}
        //EditorGUILayout.MaskField()
    }
}