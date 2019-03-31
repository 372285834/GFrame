using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace highlight
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class ActionAttribute : System.Attribute
    {
        public string name;
        public string menu;
        public TimeAction action;
        public Type actionType;
        public Type[] types;
        public bool obsolete = false;
        public ActionAttribute(string menu, Type _ac, params Type[] dType) : this(menu, _ac, false, dType) { }
        public ActionAttribute(string _menu, Type _ac, bool _obsolete, params Type[] dTypes)
        {
            obsolete = _obsolete;
            this.menu = _menu;
            this.name = _menu;
            this.actionType = _ac;
            if (name.LastIndexOf("/") > -1)
                this.name = name.Substring(name.LastIndexOf("/") + 1);
            this.types = dTypes;
            action = Activator.CreateInstance(actionType) as TimeAction;
        }
        public static string GetEnumDescription(Enum enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
            if (objs == null || objs.Length == 0)    //当描述属性没有时，直接返回名称
                return value;
            DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objs[0];
            return descriptionAttribute.Description;
        }

    }
    public enum ActionFlag
    {
      //  [Action("移动",typeof(MoveAction), typeof(ITransform), typeof(IPosition))]
        Move = 1,
    }
}