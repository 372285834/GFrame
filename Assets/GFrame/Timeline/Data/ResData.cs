using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.timeline
{
    public enum eResType
    {
        Effect,
        Animator,
        Item,
        Npc,
    }
    [Time("资源加载", typeof(ResData))]
    public sealed class ResStyle : ComponentStyle
    {
        public eResType eType;
        public string res;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public sealed class ResData : ComponentData
    {
        public System.Object asset;
        public bool isLoaded { get { return asset != null; } }
        public SceneObject obj { get { return asset as SceneObject; } }
        public Animator animator { get { return asset as Animator; } }
        public override void OnInit()
        {
            switch ((this.style as ResStyle).eType)
            {
                case eResType.Effect:
                    break;
                case eResType.Animator:
                    break;
                case eResType.Item:
                    break;
                case eResType.Npc:
                    break;
                default:
                    break;
            }
        }
    }
}