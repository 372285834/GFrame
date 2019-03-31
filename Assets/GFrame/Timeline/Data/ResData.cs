using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public class ResStyle : ComponentStyle
    {
        public string res;
    }
    public class ResData : ComponentData
    {
    }
    public class PrefabData : ResData
    {
        public SceneObject obj;

        public void SetPos(Vector3 pos)
        {
            obj.transform.position = pos;
        }
    }
}