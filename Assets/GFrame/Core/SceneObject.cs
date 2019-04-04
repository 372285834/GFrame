using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{

    public class SceneObject : Object
    {
        public Transform transform;
        public Animator animator;
        public Dictionary<string, Transform> LocatorDic = new Dictionary<string, Transform>();
        //public AnimationBox aniBox;
        public Vector3 getPosition()
        {
            return transform.position;
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
            transform.position = pos;
        }
        public void SetLocalPos(Vector3 pos)
        {
            transform.localPosition = pos;
        }
        public void SetParent(Transform t)
        {
            transform.SetParent(t);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
    }
}