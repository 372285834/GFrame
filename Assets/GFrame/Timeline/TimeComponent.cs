using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;


namespace highlight.timeline
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TimeAttribute : System.Attribute
    {
        public string menu;
        public Type dataType;
        public int capacity;
        public string name;
        public bool obsolete = false;
        public TimeAttribute(string menu, Type dType) : this(menu, 100, dType, false) { }
        public TimeAttribute(string _menu, int _capacity, Type dType, bool _obsolete)
        {
            obsolete = _obsolete;
            this.menu = _menu;
            this.name = _menu;
            if (name.LastIndexOf("/") > -1)
                this.name = name.Substring(name.LastIndexOf("/") + 1);
            capacity = _capacity;
            this.dataType = dType;
        }
    }

    public class TimeComponent
    {
        // [JsonIgnore]
        public string TypeName
        {
            get
            {
                return this.GetType().Name;
            }
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public readonly static Dictionary<Type, TimeAttribute> compAttrDic = new Dictionary<Type, TimeAttribute>();
        private TimeAttribute mAttr;
        // [JsonIgnore]
        public TimeAttribute Attr
        {
            get
            {
                if (mAttr == null)
                {
                    Type t = this.GetType();
                    compAttrDic.TryGetValue(t, out mAttr);
                    if (mAttr == null)
                    {
                        TimeAttribute[] attrs = t.GetCustomAttributes(typeof(TimeAttribute), true) as TimeAttribute[];
                        if (attrs != null && attrs.Length > 0)
                        {
                            mAttr = attrs[0];
                            compAttrDic[t] = mAttr;
                        }
                    }
                }
                return mAttr;
            }
        }

    }
}

