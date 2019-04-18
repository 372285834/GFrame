using UnityEngine;
using System.Collections;
namespace highlight
{
    public class DistanceCheck
    {
        public DistanceCheck(float minDis = 1f)
        {
            this.mUpdateDistance = 1f;
        }
        public Vector3 curPos = Vector3.zero;
        public Vector3 lastPos = Vector3.zero;
        public float mUpdateDistance = 1f;
        public bool Check(Vector3 pos)
        {
            curPos = pos;
            float dis = Mathf.Abs(Vector3.Distance(curPos, lastPos));
            if (dis < mUpdateDistance)
                return false;
            lastPos = curPos;
            return true;
        }
    }
}