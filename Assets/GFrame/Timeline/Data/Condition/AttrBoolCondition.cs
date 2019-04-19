using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Time("数据/Condition/Bool属性判断", typeof(AttrBoolCondition))]
    public class AttrBoolConditionStyle : ComponentStyle
    {
        public AttrType attrType;
        public bool value;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class AttrBoolCondition : ComponentData
    {
        public AttrType attrType;
        public bool value;
        public override void OnInit()
        {

        }
        public override TriggerStatus OnTrigger()
        {
            BoolAttr attr = this.owner.attrs.GetBoolAttr(attrType, false);
            bool v = attr == null ? false : attr.GetValue().value;
            if (v != value)
                return TriggerStatus.Failure;
            return TriggerStatus.Success;
        }
    }
}