using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using highlight;
namespace highlight.timeline
{
    public static class TimelineFactory
    {
        private static Id mIdGenerator = new Id(0);  // Id生成器
        private readonly static Dictionary<int, Timeline> mActiveDic = new Dictionary<int, Timeline>();

        private readonly static Dictionary<string, Type> typeDic = new Dictionary<string, Type>();
        public readonly static Dictionary<Type, TimeSystemAttribute> systemAttrDic = new Dictionary<Type, TimeSystemAttribute>();

        public readonly static Dictionary<TimeFlag, TimeSystem> systemDic = new Dictionary<TimeFlag, TimeSystem>();
        public static void Init()
        {
            if (typeDic.Count > 0)
                return;
            System.Type[] ctypes = typeof(TimeObject).Assembly.GetTypes();
            foreach (System.Type t in ctypes)
            {
                TimeAttribute[] attrs = t.GetCustomAttributes(typeof(TimeAttribute), true) as TimeAttribute[];
                if (attrs != null && attrs.Length > 0)
                {
                    //GEventAttribute att = attrs[0];
                    string tName = t.Name;
                    TimeComponent.compAttrDic[t] = attrs[0];
                    typeDic[tName] = t;
                }
                TimeSystemAttribute[] sysAttrs = t.GetCustomAttributes(typeof(TimeSystemAttribute), true) as TimeSystemAttribute[];
                if (sysAttrs != null && sysAttrs.Length > 0)
                {
                    TimeSystemAttribute att = sysAttrs[0];
                    systemDic[att.eFlag] = Activator.CreateInstance(t) as TimeSystem;
                    TimeSystem.systemAttrDic[t] = att;
                    typeDic[t.Name] = t;
                }

            }
        }
        static List<Timeline> releaseList = new List<Timeline>();
        public static bool AutoRelease = true;
        public static void Update()
        {
            if (mActiveDic.Count == 0)
                return;
            foreach (var tl in mActiveDic.Values)
            {
                tl.Update(Time.realtimeSinceStartup);
                if (tl.IsStopped && AutoRelease)
                    releaseList.Add(tl);
            }
            if (AutoRelease)
            {
                foreach (var tl in releaseList)
                {
                    TimelineFactory.Release(tl);
                }
                releaseList.Clear();
            }
            
        }
        public static Timeline GetActive(int id)
        {
            Timeline tl = null;
            mActiveDic.TryGetValue(id, out tl);
            return tl;
        }
        public static Timeline Creat(TimelineStyle style)
        {
            if (style == null)
                return null;
            Timeline tl = TimeObject.Create(style.getObj() as Timeline, style, null) as Timeline;
            tl.Init();
            int id = (int)mIdGenerator.generateNewId();
            tl.id = id;
            mActiveDic.Add(id, tl);
            return tl;
        }
        public static void Release(Timeline tl)
        {
            if (tl == null)
                return;
            tl.Destroy();
            mActiveDic.Remove(tl.id);
        }

        public static Type GetType(string name)
        {
            Type t = null;
            typeDic.TryGetValue(name, out t);
            return t;
        }
    }
}