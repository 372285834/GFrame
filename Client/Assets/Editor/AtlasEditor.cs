using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasEditor : Editor
{
    [MenuItem("Assets/AtlasEditor")]
    public static void AtlasEditorUpdate()
    {
        Object[] objs = Selection.objects;
        Object obj = objs[0];
        Debug.Log(obj.name);
        string targetName = obj.name;
        string[] paths =  Directory.GetFiles("Assets/BundleEditing/Atlas/" + targetName,"*.png");
        List<Object> tobjs = new List<Object>();
        foreach (var s in paths)
        {

            Object o = AssetDatabase.LoadAssetAtPath(s, typeof(Sprite));
            Debug.Log(o.name);
            tobjs.Add(o);
        }
        SpriteAtlas atlas = obj as UnityEngine.U2D.SpriteAtlas;
        SpriteAtlasExtensions.Add(atlas, tobjs.ToArray());
    }
}
