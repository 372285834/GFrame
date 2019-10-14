using highlight.tl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ConditionTool
{
    public static Dictionary<string, MethodInfo> mDic = new Dictionary<string, MethodInfo>();
    private static List<IConditionData>[] pars = new List<IConditionData>[1];
    public static bool Check(string k,List<IConditionData> cList)
    {
        if (cList.Count == 1)
            return cList[0].OnCheck();
        if(string.IsNullOrEmpty(k))
        {
            bool b = cList[0].OnCheck();
            for (int i=1;i<cList.Count;i++)
            {
                bool v = cList[i].OnCheck();
                switch (cList[i].logicType)
                {
                    case LogicType.And:
                        b = b && v;
                        break;
                    case LogicType.Or:
                        b = b || v;
                        break;
                    case LogicType.False:
                        b = b && !v;
                        break;
                    default:
                        break;
                }
            }
            return b;
        }
        MethodInfo method = null;
        if(!mDic.TryGetValue(k,out method))
        {
            Type type = Type.GetType("ConditionTool");
            method = type.GetMethod(k, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if(method != null)
            {
                mDic[k] = method;
            }
        }
        if(method != null)
        {
            pars[0] = cList;
            object o = method.Invoke(null, pars);
            pars[0] = null;
            return (bool)o;
        }
        return false;
    }
}
