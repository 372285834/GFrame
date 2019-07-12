using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    public enum TriggerType
    {
        不触发 = 0,
        失败后继续 = 1,
        失败后停止 = 2,
    }
    public enum TriggerStatus
    {
        InActive = 0,
        Failure = 1,
        Success = 2,
        Running = 3
    }
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
            if (dType == null)
                dType = typeof(ComponentData);
            this.dataType = dType;
        }
    }
    public abstract class Component : Object
    {
        public int index;
        public TriggerStatus status = TriggerStatus.InActive;
        public TimeObject timeObject { protected set; get; }
        public Timeline root { get { return this.timeObject.root; } }
        public Role owner { get { return this.root.owner; } }
        public TimeStyle timeStyle { get { return this.timeObject.timeStyle; } }
        public string name { get { return this.timeObject.name; } }
        public List<ComponentData> ComponentList { get { return timeObject.ComponentList; } }
        #region virtual Function
        public virtual void OnInit() { }
        public virtual bool OnTrigger() { return true; }
        public virtual void OnFinish() { } // event完成
        public virtual void OnStop() { } //timeline 完成
        #endregion
    }
    public abstract class ComponentStyle : Object
    {
        public TriggerType tType = TriggerType.不触发;
        [Newtonsoft.Json.JsonIgnore]
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
        [Newtonsoft.Json.JsonIgnore]
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
            this.tType = (TriggerType)EditorGUILayout.EnumPopup("触发类型：", tType);
        }
#endif
    }
    public class ComponentData : Component
    {
        public ComponentStyle style { private set; get; }
        public TriggerStatus Trigger()
        {
            status = TriggerStatus.Success;
            TriggerType tType = style.tType;
            if (tType == TriggerType.不触发)
                return status;
            if (OnTrigger())
                return status;
            if (tType == TriggerType.失败后继续)
                status = TriggerStatus.Running;
            else
                status = TriggerStatus.Failure;
            return status;
        }
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

