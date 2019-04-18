using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public class NodeControl : MNode
    {
        public Dictionary<string, Transform> Nodes = new Dictionary<string, Transform>();
        public void Awake()
        {
            if (Nodes.Count > 0)
                return;
            Transform[] tfs = this.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tfs.Length; i++)
            {
                Nodes[tfs[i].name] = tfs[i];
            }
        }
        public static NodeControl Add(GameObject go)
        {
            NodeControl control = go.AddComp<NodeControl>();
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
        public void SetVisible(string name,bool b)
        {
            var node = Get(name);
            if (node != null)
                node.gameObject.SetActive(b);
        }
    }
}