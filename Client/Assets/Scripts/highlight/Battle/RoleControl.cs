using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public class RoleControl : MonoBehaviour, ISerializeField
    {
        public int id;
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }
        public Dictionary<string, Transform> Nodes = new Dictionary<string, Transform>();
        public Animator mAnimator;
        public GameObject fbx;
        public string curClip;
        public void Awake()
        {
            Transform[] tfs = this.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tfs.Length; i++)
            {
                Nodes[tfs[i].name] = tfs[i];
            }
        }
        public virtual void SerializeFieldInfo()
        {
            if (mAnimator == null)
                mAnimator = this.GetComponentInChildren<Animator>();
            if (mAnimator != null)
                fbx = mAnimator.gameObject;
        }
        public static RoleControl Add(GameObject go)
        {
            RoleControl control = go.AddComp<RoleControl>();
            return control;
        }

        public Transform Get(string name)
        {
            Transform tf = null;
            Nodes.TryGetValue(name, out tf);
            return tf;
        }
        public GameObject GetGo(string name)
        {
            Transform tf = null;
            Nodes.TryGetValue(name, out tf);
            return tf.gameObject;
        }
        public Animator GetAnimator(string name)
        {
            Transform tf = Get(name);
            if (tf != null)
                return tf.GetComponent<Animator>();
            return null;
        }
        public void SetVisible(string name, bool b)
        {
            var node = Get(name);
            if (node != null)
                node.gameObject.SetActive(b);
        }
    }
}