using highlight.timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace highlight
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TimeAttribute : System.Attribute
    {
        public string menu;
        public Type dataType;
        public int capacity;
        public string name;
        public bool obsolete = false;
        public TimeAttribute(string menu, Type dType = null) : this(menu, 100, dType, false) { }
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
    public abstract class Component : Object
    {
        public int index;
        public ResData res { get { return timeObject.resData; } }
        public TimeObject timeObject { protected set; get; }
        public Timeline root { get { return this.timeObject.root; } }
        public SceneObject owner { get { return this.root.owner; } }
        public TimeStyle timeStyle
        {
            get
            {
                return this.timeObject.timeStyle;
            }
        }
        public List<ComponentData> GetComponents { get { return timeObject.GetComponents; } }
        #region virtual Function
        public virtual void OnInit() { }
        public virtual void OnDestroy() { }// timeline 销毁
        public virtual void OnTrigger() { }
        #endregion
    }
    public abstract class ComponentStyle : Object
    {
        // [JsonIgnore]
        public string TypeName
        {
            get
            {
                return this.GetType().FullName;
            }
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public readonly static Dictionary<Type, TimeAttribute> compAttrDic = new Dictionary<Type, TimeAttribute>();
        protected TimeAttribute mAttr;
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
#if UNITY_EDITOR
        public virtual void OnInspectorGUI()
        {

        }
#endif
    }
    public class ComponentData : Component
    {
        public ComponentStyle style { private set; get; }

        private readonly static Dictionary<string, ObjectPool> CompPoolDic = new Dictionary<string, ObjectPool>();

        public static ComponentData Get(ComponentStyle comp, TimeObject t)
        {
            if (comp == null)
                return null;
            ComponentData data = getPool(comp).Get(comp.Attr.dataType) as ComponentData;
            data.timeObject = t;
            data.style = comp;
            return data;
        }
        public static void Release(ComponentData logic)
        {
            if (logic == null)
                return;
            getPool(logic.style).Release(logic);
            logic.timeObject = null;
            logic.style = null;
        }
        static ObjectPool getPool(ComponentStyle data)
        {
            ObjectPool pool = null;
            CompPoolDic.TryGetValue(data.TypeName, out pool);
            if (pool == null)
            {
                pool = new ObjectPool();
                CompPoolDic[data.TypeName] = pool;
            }
            return pool;
        }
    }
}

