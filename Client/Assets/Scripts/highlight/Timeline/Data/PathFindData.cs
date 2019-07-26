using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/寻路", typeof(PathFindData))]
    public class PathFindStyle : ComponentStyle
    {
        public bool isAll = true;
        public int areaMask;
        public int agentTypeID;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {

        }
#endif
    }
    public class PathFindData : ComponentData
    {
        public NavMeshPath path = new NavMeshPath();
        public PathFindStyle mStyle { get { return this.style as PathFindStyle; } }
        public int curIndex = 0;
        public override bool OnTrigger()
        {
            curIndex = 0;
            this.path.ClearCorners();
            return true;
        }
        public NavMeshQueryFilter filter
        {
            get
            {
                PathFindStyle s = this.style as PathFindStyle;
                return new NavMeshQueryFilter() { areaMask = s.areaMask, agentTypeID = s.agentTypeID };
            }
        }

    }
}