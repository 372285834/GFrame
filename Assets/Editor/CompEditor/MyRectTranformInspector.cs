using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
	
[CustomEditor(typeof(RectTransform))]
public class MyTest : DecoratorEditor
{
    public MyTest() : base("UnityEditor.RectTransformEditor") { }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Adding this button"))
        {
            Debug.Log("Adding this button");
        }
    }
}
