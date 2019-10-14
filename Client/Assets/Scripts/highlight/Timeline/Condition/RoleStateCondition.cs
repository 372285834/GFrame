using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("条件/RoleState判断", typeof(RoleStateCondition))]
    public class RoleStateConditionStyle : ConditionStyle
    {
        public RoleState state;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
             this.state = (RoleState)EditorGUILayout.EnumPopup("状态：", this.state);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class RoleStateCondition : ComponentData<RoleStateConditionStyle>, IConditionData
    {
        public LogicType logicType { get { return mStyle.logicType; } }

        public bool OnCheck()
        {
            return this.owner.state == mStyle.state;
        }
    }
}