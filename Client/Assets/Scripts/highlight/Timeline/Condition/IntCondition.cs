
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
    public class IntConditionStyle : ConditionStyle
    {
        public AttrCompareType cType;
        public int value;
        public bool isObs;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            this.cType = (AttrCompareType)EditorGUILayout.EnumPopup("比较类型：", this.cType);
            this.value = EditorGUILayout.IntField("value：", this.value);
           // this.isObs = EditorGUILayout.Toggle("注册事件：", this.isObs);
        }
#endif

        public static bool CompareValue(int v, AttrCompareType cType, int value)
        {
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
            return b;
        }
    }
}