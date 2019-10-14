using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace highlight.tl
{
    public static class TimelineFactory
    {
        private static Id mIdGenerator = new Id(0);  // Id生成器
        public readonly static Dictionary<int, Timeline> mActiveDic = new Dictionary<int, Timeline>();

        private readonly static Dictionary<string, Type> typeDic = new Dictionary<string, Type>();
        //public readonly static Dictionary<ActionFlag, ActionAttribute> actionAttrDic = new Dictionary<ActionFlag, ActionAttribute>();
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
                    string tName = t.FullName;
                    ComponentStyle.compAttrDic[t] = attrs[0];
                    typeDic[tName] = t;
                }
                ActionAttribute[] actionAttrs = t.GetCustomAttributes(typeof(ActionAttribute), true) as ActionAttribute[];
                if (actionAttrs != null && actionAttrs.Length > 0)
                {
                    //GEventAttribute att = attrs[0];
                    string tName = t.FullName;
                    ActionStyle.actionAttrDic[tName] = actionAttrs[0];
                    typeDic[tName] = t;
                }
            }
            //ActionFlag acFlag = ActionFlag.Node;
            //Type et = typeof(ActionFlag);
            //FieldInfo[] fields = et.GetFields();
            //foreach ( FieldInfo fi in fields )
            //{
            //    ActionFlag flag = (ActionFlag)fi.GetValue(acFlag);
            //    ActionAttribute[] attrs = fi.GetCustomAttributes(typeof(ActionAttribute), true) as ActionAttribute[];
            //    if (attrs != null && attrs.Length > 0)
            //    {
            //        ActionAttribute att = attrs[0];
            //        actionAttrDic[flag] = att;
            //    }
            //}
        }
        /*
        static List<Timeline> destroyList = new List<Timeline>();
        public static void Update(float time)
        {
            if (mActiveDic.Count == 0)
                return;
            foreach (var tl in mActiveDic.Values)
            {
                tl.Update(time);
                if (tl.IsStopped && tl.DestroyOnStop)
                    destroyList.Add(tl);
            }
            if(destroyList.Count > 0)
            {
                foreach (var tl in destroyList)
                {
                    mActiveDic.Remove(tl.onlyId);
                    tl.Destroy();
                }
                destroyList.Clear();
            }
        }
        public static Timeline GetActive(int id)
        {
            Timeline tl = null;
            mActiveDic.TryGetValue(id, out tl);
            return tl;
        }
        */
        public static Timeline Creat(string url,Role owner)
        {
            TimelineStyle style = LoadTimeStyle.Load(url);
            return Creat(style, owner);
        }
        public static Timeline Creat(TimelineStyle style, Role owner)
        {
            if (style == null)
            {
                return null;
            }
            Timeline tl = style.Creat();
            tl.SetOnlyId(mIdGenerator.generateNewId());
            mActiveDic.Add(tl.onlyId, tl);
            tl.owner = owner;
            tl.Init();
            return tl;
        }
        public static void Destroy(Timeline tl)
        {
            if (tl == null)
                return;
            mActiveDic.Remove(tl.onlyId);
            tl.Destroy();
        }
        public static void GetByRole(List<Timeline> list,Role role)
        {
            list.Clear();
            foreach (var tl in mActiveDic.Values)
            {
                if (tl.owner == role)
                    list.Add(tl);
            }
        }
        public static Type GetType(string name)
        {
            Type t = null;
            typeDic.TryGetValue(name, out t);
            return t;
        }
    }
}