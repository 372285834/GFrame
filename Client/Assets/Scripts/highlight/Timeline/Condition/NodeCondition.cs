using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("条件/节点状态判断", typeof(NodeCondition))]
    public class NodeConditionStyle : ConditionStyle
    {
        public string nodeName;
        public TriggerStatus status;
        public bool equal = true;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.nodeName = EditorGUILayout.TextField("节点名：", this.nodeName);
            this.status = (TriggerStatus)EditorGUILayout.EnumPopup("节点状态：", this.status);
            this.equal = EditorGUILayout.Toggle("相等", equal);
        }
#endif
    }
    public class NodeCondition : ConditionData
    {
        public string nodeName;
        public TriggerStatus nodeStatus;
        public bool equal;
        public override void OnInit()
        {
            NodeConditionStyle s = this.GetStyle<NodeConditionStyle>();
            nodeName = s.nodeName;
            nodeStatus = s.status;
            equal = s.equal;
            base.OnInit();
        }
        public override bool OnCheck()
        {
            TimeObject obj = this.root.FindObj(this.nodeName);
            return (obj.Status == this.nodeStatus) == equal;
        }
    }
}