using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight
{
    public class ResStyle : ComponentStyle
    {
        public string res;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class ResData : ComponentData
    {
    }
    public class PrefabData : ResData
    {
        public SceneObject obj;

        public void SetPos(Vector3 pos)
        {
            obj.transform.position = pos;
        }
        public void SetParent(Transform t)
        {
            obj.transform.SetParent(t);
        }
    }
}