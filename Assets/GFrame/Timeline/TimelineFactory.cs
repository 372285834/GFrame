using highlight.timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace highlight.timeline
{
    public static class TimelineFactory
    {
        private static Id mIdGenerator = new Id(0);  // Id生成器
        private readonly static Dictionary<int, Timeline> mActiveDic = new Dictionary<int, Timeline>();

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
                    string tName = t.Name;
                    ComponentStyle.compAttrDic[t] = attrs[0];
                    typeDic[tName] = t;
                }
                ActionAttribute[] actionAttrs = t.GetCustomAttributes(typeof(ActionAttribute), true) as ActionAttribute[];
                if (actionAttrs != null && actionAttrs.Length > 0)
                {
                    //GEventAttribute att = attrs[0];
                    string tName = t.Name;
                    ActionStyle.actionAttrDic[t] = actionAttrs[0];
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
        public static Timeline Creat(TimelineStyle style)
        {
            if (style == null)
                return null;
            Timeline tl = style.Creat();
            int id = (int)mIdGenerator.generateNewId();
            tl.SetOnlyId(id);
            mActiveDic.Add(id, tl);
            tl.Init();
            return tl;
        }

        public static Type GetType(string name)
        {
            Type t = null;
            typeDic.TryGetValue(name, out t);
            return t;
        }
    }
}