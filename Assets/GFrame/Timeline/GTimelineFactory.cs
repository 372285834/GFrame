using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        }
        public static GEvent GetEvent(GEventStyle data)
        {
            if (data == null)
                return null;
            GEvent evt = getPool(data).Get(data.Attr.dataType) as GEvent;
            return evt;
        }
        public static void ReleaseEvent(GEvent evt)
        {
            if (evt == null)
                return;
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

    }
    /*
    [Serializable]
    public class GJsonInfo
    {
        public string type;
        public string json;
        public int count;
        public Type sType
        {
            get { return GTimelineFactory.GetType(type); }
        }
        public static string Serialize(List<GJsonInfo> infos)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < infos.Count; i++)
            {
                sb.Append(infos[i].json + "\t");
                sb.Append(infos[i].type + "\t");
                sb.Append(infos[i].count + "\t");
                sb.Append("\n");
            }
            return sb.ToString();
        }
        public static List<GJsonInfo> DeSerializeToList(string txts)
        {
            List<GJsonInfo> infos = new List<GJsonInfo>();
            string[] lines = txts.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;
                string[] rows = lines[i].Split('\t');
                GJsonInfo info = new GJsonInfo();
                info.json = rows[0];
                info.type = rows[1];
                info.count = Int32.Parse(rows[2]);
                infos.Add(info);
            }
            return infos;
        }
        public static string Serialize(GTimelineStyle style)
        {
            List<GJsonInfo> infos = new List<GJsonInfo>();
            serialize(style, infos);
            return GJsonInfo.Serialize(infos);
        }
        static void serialize(GEventStyle style, List<GJsonInfo> infos)
        {
            string json = JsonUtility.ToJson(style);
            GJsonInfo info = new GJsonInfo();
            info.type = style.typeName;
            info.count = style.styles.Count;
            info.json = json;
            infos.Add(info);
            for (int i = 0; i < style.styles.Count; i++)
            {
                serialize(style.styles[i], infos);
            }
        }
        public static GEventStyle DeSerialize(string json)
        {
            List<GJsonInfo> infos = GJsonInfo.DeSerializeToList(json);
            GJsonInfo info = infos[0];
            GEventStyle tl = JsonUtility.FromJson(infos[0].json, info.sType) as GEventStyle;
            int index = 1;
            deserialize(tl, infos, info.count, ref index);
            return tl;
        }
        static void deserialize(GEventStyle style, List<GJsonInfo> infos, int count, ref int index)
        {
            if (count <= 0)
                return;
            while (style.styles.Count < count)
            {
                GJsonInfo info = infos[index];
                Type type = GTimelineFactory.GetType(info.type);
                try
                {
                    GEventStyle evt = JsonUtility.FromJson(info.json, type) as GEventStyle;
                    style.styles.Add(evt);
                    index++;
                    deserialize(evt, infos, info.count, ref index);
                }
                catch (Exception e)
                {
                    Debug.LogError(index + "\n" + info.json + "\n" + e.StackTrace + "\n" + e.Message);
                    return;
                }
            }
        }
    }
        */
}