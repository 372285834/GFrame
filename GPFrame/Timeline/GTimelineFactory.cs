using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    public static class GTimelineFactory
    {
        private static Id mIdGenerator = new Id(0);  // Id生成器
        private readonly static Dictionary<string, GTimelineStyle> mStyleDic = new Dictionary<string, GTimelineStyle>();
        private readonly static Dictionary<string, ObjectPool> eventPoolDic = new Dictionary<string, ObjectPool>();
        private readonly static Dictionary<int, GTimeline> mActiveDic = new Dictionary<int, GTimeline>();

        private readonly static Dictionary<string, Type> typeDic = new Dictionary<string, Type>();
        public readonly static Dictionary<Type, GEventAttribute> eventAttrDic = new Dictionary<Type, GEventAttribute>();

        public static void Init()
        {
            if (typeDic.Count > 0)
                return;
            System.Type[] ctypes = typeof(GEvent).Assembly.GetTypes();
            foreach (System.Type t in ctypes)
            {
                GEventAttribute[] attrs = t.GetCustomAttributes(typeof(GEventAttribute), true) as GEventAttribute[];
                if (attrs != null)
                {
                    if(attrs.Length > 0)
                    {
                        GEventAttribute att = attrs[0];
                        string tName = t.Name;
                        eventAttrDic[t] = attrs[0];
                        typeDic[tName] = t;
                        if(t == typeof(GTimelineStyle))
                            eventPoolDic[tName] = new ObjectPool(actionOnGet, actionOnRelease);
                        else
                            eventPoolDic[tName] = new ObjectPool();// attrs[0].dataType;
                    }
                }
            }
        }
        static void actionOnGet(object obj)
        {
            GTimeline tl = (GTimeline)obj;
            int id = (int)mIdGenerator.generateNewId();
            tl.SetId(id);
            mActiveDic.Add(id, tl);
        }
        static void actionOnRelease(object obj)
        {
            GTimeline tl = (GTimeline)obj;
            mActiveDic.Remove(tl.id);
        }
        static List<GTimeline> releaseList = new List<GTimeline>();
        public static bool AutoRelease = true;
        public static void Update()
        {
            if (mActiveDic.Count == 0)
                return;
            foreach (var tl in mActiveDic.Values)
            {
                tl.Update();
                if (tl.IsStopped && AutoRelease)
                    releaseList.Add(tl);
            }
            if (AutoRelease)
            {
                foreach (var tl in releaseList)
                {
                    GTimelineFactory.ReleaseTimeline(tl);
                }
                releaseList.Clear();
            }
            
        }
        public static void FixedUpdate()
        {
            if (mActiveDic.Count == 0)
                return;
            foreach (var tl in mActiveDic.Values)
            {
                tl.FixedUpdate();
            }
        }
        public static GTimeline GetActiveTimeline(int id)
        {
            GTimeline tl = null;
            mActiveDic.TryGetValue(id, out tl);
            return tl;
        }
        public static Type GetType(string name)
        {
            Type t = null;
            typeDic.TryGetValue(name, out t);
            return t;
        }
        public static GTimelineStyle GetStyle(string styleName)
        {
            GTimelineStyle s = null;
            mStyleDic.TryGetValue(styleName, out s);
            return s;
        }
        public static GTimeline CreatTimeline(string styleName)
        {
            GTimelineStyle s = GetStyle(styleName);
            return CreatTimeline(s);
        }
        public static GTimeline CreatTimeline(GTimelineStyle style)
        {
            if (style == null)
                return null;
            GTimeline tl = GEvent.Create(null, style, null) as GTimeline;
            tl.Init();
            return tl;
        }
        public static void ReleaseTimeline(GTimeline tl)
        {
            if (tl == null)
                return;
            tl.Destroy();
            GTimelineFactory.ReleaseEvent(tl);
        }
        public static GEvent GetEvent(GEventStyle data)
        {
            GEvent evt = getPool(data).Get(data.Attr.dataType) as GEvent;
            return evt;
        }
        public static void ReleaseEvent(GEvent evt)
        {
            getPool(evt.mStyle).Release(evt);
        }
        static ObjectPool getPool(GEventStyle data)
        {
            ObjectPool pool = null;
            eventPoolDic.TryGetValue(data.typeName, out pool);
            return pool;
        }
        public static GEventAttribute GetAttr(GEventStyle data)
        {
            GEventAttribute attr = null;
            eventAttrDic.TryGetValue(data.GetType(), out attr);
            return attr;
        }


        public static string Serialize(GEventStyle style)
        {
            GTimelineFactory.Serialize(style.styles, style.jsons, style.types);
            return JsonUtility.ToJson(style);
        }
        public static GTimelineStyle DeSerialize(string name,string json)
        {
            GTimelineStyle evt = JsonUtility.FromJson(json, typeof(GTimelineStyle)) as GTimelineStyle;
            evt.name = name;
            GTimelineFactory.Deserialize(evt.styles, evt.jsons, evt.types);
            return evt;
        }
        public static void DeSerialize(GEventStyle style)
        {
            GTimelineFactory.Deserialize(style.styles, style.jsons, style.types);
        }
        public static void Serialize(List<GEventStyle> styles, List<string> jsons, List<string> types)
        {
            jsons.Clear();
            types.Clear();
            for (int i = 0; i < styles.Count; i++)
            {
                Serialize(styles[i]);
                string json = JsonUtility.ToJson(styles[i]);
                jsons.Add(json);
                types.Add(styles[i].typeName);
            }
        }
        public static void Deserialize(List<GEventStyle> styles, List<string> jsons, List<string> types)
        {
            styles.Clear();
            for (int i = 0; i < jsons.Count; i++)
            {
                Type type = GTimelineFactory.GetType(types[i]);
                if (type == null)
                    continue;
                try
                {
                    GEventStyle evt = JsonUtility.FromJson(jsons[i], type) as GEventStyle;
                    DeSerialize(evt);
                    styles.Add(evt);
                }
                catch(Exception e)
                {
                    Debug.LogError(jsons[i] + "\n" + types[i]+ "\n" + e.StackTrace + "\n" + e.Message);
                }
            }
        }

    }
}