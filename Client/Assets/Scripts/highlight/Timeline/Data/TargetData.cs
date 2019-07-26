using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    public enum TargetType
    {
        Self,
        Target,
        Parent,
    }
    [Time("数据/目标", typeof(TargetData))]
    public class TargetStyle : ComponentStyle
    {
        public TargetType targetType;
        public int index;
#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {

        }
#endif
    }
    public class TargetData : ComponentData
    {
        public Role obj
        {
            get
            {
                Role _obj = null;
                TargetStyle s = this.style as TargetStyle;
                switch (s.targetType)
                {
                    case TargetType.Self:
                        _obj = this.timeObject.role;
                        break;
                    case TargetType.Target:
                        _obj = this.target.getObj(s.index);
                        break;
                    case TargetType.Parent:
                        _obj = this.timeObject.parent.role;
                        break;
                    default:
                        break;
                }
                return _obj;
            }
        }
    }
}