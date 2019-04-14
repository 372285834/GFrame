using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{

    public class SceneObject : Object
    {
        public string state;
        public bool isClear = false;
        public Transform root;
        public Animator animator;
        public Dictionary<string, Transform> LocatorDic = new Dictionary<string, Transform>();
        //public AnimationBox aniBox;
        public void Init(GameObject go)
        {
            isClear = false;
            this.root = go.transform;
            this.animator = go.GetComponentInChildren<Animator>(true);
            Transform[] tfs = go.GetComponentsInChildren<Transform>(true);
            for(int i=0;i<tfs.Length;i++)
            {
                LocatorDic[tfs[i].name] = tfs[i];
            }
        }
        public Vector3 getPosition()
        {
            return root.position;
        }
        public Transform getLocator(string name)
        {
            return null;
        }
        public void PlayAction()
        {

        }
        public void SetPos(Vector3 pos)
        {
            root.position = pos;
        }
        public void SetLocalPos(Vector3 pos)
        {
            root.localPosition = pos;
        }
        public void SetParent(Transform t)
        {
            root.SetParent(t);
            root.localPosition = Vector3.zero;
            root.localScale = Vector3.one;
            root.localRotation = Quaternion.identity;
        }
        public void Clear()
        {
            isClear = true;
        }
    }
}