using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    [Time("数据/轨迹", typeof(TrailData))]
    public class TrailStyle : ComponentStyle
    {
        
    }
    public class TrailData : ComponentData, IEvaluate
    {
        public Vector3 Evaluate(Vector3 start, Vector3 end, float time)
        {
            Vector3 dir = end - start;
            return Vector3.Lerp(start, end, time);
        }
    }

}