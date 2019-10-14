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
            this.isAll = EditorGUILayout.Toggle("isAll：", isAll);
            this.areaMask = EditorGUILayout.IntField("areaMask：", areaMask);
            this.agentTypeID = EditorGUILayout.IntField("agentTypeID：", agentTypeID);
        }
#endif
    }
    public class PathFindData : ComponentData<PathFindStyle>
    {
        public NavMeshPath path = new NavMeshPath();
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
                PathFindStyle s = mStyle;
                return new NavMeshQueryFilter() { areaMask = s.areaMask, agentTypeID = s.agentTypeID };
            }
        }

    }
}