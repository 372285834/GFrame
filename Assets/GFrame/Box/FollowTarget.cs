using System;
using UnityEngine;


//namespace UnityStandardAssets.Utility
//{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = Vector3.zero;
        public float offForward = 0f;
        public int scaleIdx = 0;
        //private Vector3 sScale = Vector3.one;
        //void Start()
        //{
        //    sScale = this.transform.localScale;
        //}
        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 t = target.position + offset;
                if (offForward != 0f)
                    t += transform.forward * offForward;
                transform.position = t;
            }
            //if (scaleIdx > 0 && Frame.ThreeLockCamera.Main != null)
            //{
            //    float scale = Frame.ThreeLockCamera.GetFovCurve(scaleIdx);
            //    this.transform.localScale = new Vector3(scale, scale, scale);
            //}
        }
    }
//}
