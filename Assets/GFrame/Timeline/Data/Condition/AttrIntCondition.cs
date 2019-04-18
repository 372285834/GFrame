
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    public enum AttrCompareType
    {
        大于,
        大于等于,
        小于,
        小于等于,
        等于,
    }
    [Time("数据/数值属性", typeof(AttrIntCondition))]
    public class AttrIntConditionStyle : ComponentStyle
    {
        public int value;
        public IntAttrType attrType;
        public AttrCompareType cType;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            // this.eType = (eResType)EditorGUILayout.EnumPopup("类型：", this.eType);
            // this.res = EditorGUILayout.TextField("资源名：", this.res);
        }
#endif
    }
    public class AttrIntCondition : ComponentData
    {
        public int value;
        public IntAttrType attrType;
        public AttrCompareType cType;
        public override void OnInit()
        {

        }
        public override TriggerStatus OnTrigger()
        {
            IntAttr attr = this.owner.attrs.GetIntAttr(attrType);
            int v = attr.GetValue().value;
            bool b = false;
            switch (cType)
            {
                case AttrCompareType.大于:
                    b = v > value;
                    break;
                case AttrCompareType.大于等于:
                    b = v >= value;
                    break;
                case AttrCompareType.小于:
                    b = v < value;
                    break;
                case AttrCompareType.小于等于:
                    b = v <= value;
                    break;
                case AttrCompareType.等于:
                    b = v == value;
                    break;
                default:
                    break;
            }
            return b ? TriggerStatus.Success : TriggerStatus.Failure;
        }
    }
}