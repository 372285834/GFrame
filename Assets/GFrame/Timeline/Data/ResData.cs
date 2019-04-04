using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight
{
    public enum eResType
    {
        Effect,
        Animator,
        Item,
        Npc,
    }
    [Time("资源加载", typeof(ResData))]
    public abstract class ResStyle : ComponentStyle
    {
        public string res;
        public eResType eType;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            this.res = EditorGUILayout.TextField("资源名：", this.res);
            this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
        }
#endif
    }
    public abstract class ResData : ComponentData
    {
        public System.Object asset;
        public SceneObject obj { get { return asset as SceneObject; } }
        public Animator animator { get { return asset as Animator; } }
        public override void OnInit()
        {
            if (asset == null)
                this.timeObject.SetActive(false);
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
        void loadComplete()
        {
            this.timeObject.SetActive(asset != null);
        }
    }
}