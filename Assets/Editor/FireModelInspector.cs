using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
	
//[CustomEditor(typeof(Mesh))]
public class FireModelInspector : DecoratorEditor
{
    Mesh mPreviewMesh;
    Material mPreviewMaterial;
    PreviewRenderUtility mPreviewRenderUtility;

    public FireModelInspector() : base("UnityEditor.ModelInspector")
    {
    }
    static MethodInfo ObjectPreviewDrawPreview = typeof(ObjectPreview).GetMethod("DrawPreview", BindingFlags.Static | BindingFlags.NonPublic);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Adding this button"))
        {
            Debug.Log("Adding this button");
        }
    }
     public override sealed void DrawPreview(Rect previewArea)
     {
         ObjectPreviewDrawPreview.Invoke(null, new object[] { this, previewArea, this.targets });
     }
     public override sealed void OnInteractivePreviewGUI(Rect r, GUIStyle background)
     {
         if (Event.current.type != EventType.Repaint)
             return;

         mPreviewMesh = base.target as Mesh;
         //this.OnPreviewGUI(r, background);
         if (mPreviewRenderUtility == null)
         {
             mPreviewRenderUtility = new PreviewRenderUtility();
             mPreviewRenderUtility.camera.farClipPlane = 500;
             mPreviewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;
             mPreviewRenderUtility.camera.transform.position = new Vector3(0, 0, -10);
             //mPreviewMaterial = new Material(Shader.Find("Fire/UVshader"));
             var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
             //var meshFilter = go.GetComponent<MeshFilter>();
             //mPreviewMesh = meshFilter.sharedMesh;
             mPreviewMaterial = go.GetComponent<Renderer>().sharedMaterial;
             DestroyImmediate(go);
         }
         //var drawRect = new Rect(0, 0, 100, 100);
         mPreviewRenderUtility.BeginPreview(r, background);
         InternalEditorUtility.SetCustomLighting(mPreviewRenderUtility.lights, new Color(0.6f, 0.6f, 0.6f, 1f));
         mPreviewRenderUtility.DrawMesh(mPreviewMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(30, 45, 0), Vector3.one), mPreviewMaterial, 0);
        
         mPreviewRenderUtility.camera.Render();
         mPreviewRenderUtility.EndAndDrawPreview(r);
         InternalEditorUtility.RemoveCustomLighting();
         //GUI.Box(drawRect, texture);
     }

}
