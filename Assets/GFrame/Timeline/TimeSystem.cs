using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.timeline
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TimeSystemAttribute : System.Attribute
    {
        public string name;
        public string menu;
        public TimeFlag eFlag;
        public Type[] types;
        public bool obsolete = false;
        public TimeSystemAttribute(string menu, TimeFlag _eFlag, params Type[] dType) : this(menu, _eFlag, false, dType) { }
        public TimeSystemAttribute(string _menu, TimeFlag _eFlag, bool _obsolete, params Type[] dTypes)
        {
            obsolete = _obsolete;
            this.menu = _menu;
            this.name = _menu;
            this.eFlag = _eFlag;
            if (name.LastIndexOf("/") > -1)
                this.name = name.Substring(name.LastIndexOf("/") + 1);
            this.types = dTypes;
        }
    }
    public class TimeSystem
    {
        #region virtual Function
        public virtual void OnInit(TimeObject obj) { }
        public virtual void OnDestroy(TimeObject obj) { }// timeline 销毁

        public virtual void OnTrigger(TimeObject obj, int framesSinceTrigger, float timeSinceTrigger) { }
        public virtual void OnUpdateEvent(TimeObject obj, int framesSinceTrigger, float timeSinceTrigger) { }
        public virtual void OnFinish(TimeObject obj) { } // event完成
        public virtual void OnStop(TimeObject obj) { } //timeline 完成
        public virtual void OnResume(TimeObject obj) { }
        public virtual void OnPause(TimeObject obj) { }
        #endregion

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public static TimeSystem Default = new TimeSystem();
        public static TimeSystem Get(TimeFlag eFlag)
        {
            TimeSystem sys = null;
            TimelineFactory.systemDic.TryGetValue(eFlag, out sys);
            if (sys == null)
                return Default;
            return sys;
        }
        public readonly static Dictionary<Type, TimeSystemAttribute> systemAttrDic = new Dictionary<Type, TimeSystemAttribute>();
        private TimeSystemAttribute mAttr;
        // [JsonIgnore]
        public TimeSystemAttribute Attr
        {
            get
            {
                if (mAttr == null)
                {
                    Type t = this.GetType();
                    systemAttrDic.TryGetValue(t, out mAttr);
                    if (mAttr == null)
                    {
                        TimeSystemAttribute[] attrs = t.GetCustomAttributes(typeof(TimeSystemAttribute), true) as TimeSystemAttribute[];
                        if (attrs != null && attrs.Length > 0)
                        {
                            mAttr = attrs[0];
                            systemAttrDic[t] = mAttr;
                        }
                    }
                }
                return mAttr;
            }
        }
    }
}