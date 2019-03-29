using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{

    public class SceneObject : Object
    {
        public Transform transform;
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
    }
}