﻿using highlight.timeline;
using System;
using System.Collections;
using System.Collections.Generic;

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
            if (this.dataType == null)
                this.dataType = typeof(ComponentData);
        }
    }

    public abstract class ComponentStyle
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
    public class ComponentData
    {
        public int index;
        public TimeObject timeObject { private set; get; }
        public Timeline root { get { return this.timeObject.root; } }
        public SceneObject owner { get { return this.root.owner; } }
        public PrefabData prefabData { get { return timeObject.resData as PrefabData; } }
        public AnimatorData animatorData { get { return timeObject.resData as AnimatorData; } }
        public TimeStyle timeStyle
        {
            get
            {
                return this.timeObject.timeStyle;
            }
        }
        public List<ComponentData> GetComponents { get { return timeObject.GetComponents; } }
        public ComponentStyle style { private set; get; }
        #region virtual Function
        public virtual void OnInit() { }
        public virtual void OnDestroy() { }// timeline 销毁
        public virtual void OnTrigger() { }
        public virtual void OnFinish() { } // event完成
        public virtual void OnStop() { } //timeline 完成
        public virtual void OnResume() { }
        public virtual void OnPause() { }
        #endregion


        private readonly static Dictionary<string, ObjectPool> logicPoolDic = new Dictionary<string, ObjectPool>();

        public static ComponentData Get(ComponentStyle comp, TimeObject t)
        {
            if (comp == null)
                return null;
            ComponentData logic = getPool(comp).Get(comp.Attr.dataType) as ComponentData;
            logic.timeObject = t;
            logic.style = comp;
            return logic;
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
            logicPoolDic.TryGetValue(data.TypeName, out pool);
            if (pool == null)
            {
                pool = new ObjectPool();
                logicPoolDic[data.TypeName] = pool;
            }
            return pool;
        }
    }
}

