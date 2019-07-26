using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using highlight;

public class MUIFunction : MonoBehaviour
{
    public Dictionary<string, MonoBehaviour> mComDic = new Dictionary<string, MonoBehaviour>();
	// Use this for initialization
    public static MUIFunction AddComp(GameObject go)
    {
        MUIFunction ui = go.AddComp<MUIFunction>();
        if (ui != null)
            ui.InitComp();
        return ui;
    }
    public void InitComp() 
    {
        if (mComDic.Count > 0)
            return;
        MonoBehaviour[] mos = this.GetComponentsInChildren<MonoBehaviour>(true);
        int length = mos.Length;
        for (int i = 0; i < length; i++)
        {
            MonoBehaviour mono = mos[i];
            if (mono == null)
                continue;
            string key = mono.name;
            mComDic[key] = mono;
        }
    }
    public static MUIFunction BindingLua(GameObject go, XLua.LuaTable tb)
    {
        MUIFunction ui = go.AddComp<MUIFunction>();
        ui.BindingLua(tb);
        return ui;
    }
    public void BindingLua(XLua.LuaTable tb)
    {
        InitComp();
        foreach (var k in mComDic.Keys)
        {
            tb.SetInPath(k, mComDic[k]);
        }
    }
    public int Size
    {
        get { return mComDic != null ? mComDic.Count : 0; }
    }
    public MonoBehaviour GetCompByType(string name, string type)
    {
        MonoBehaviour mono = GetComp(name);
        if (mono != null && !string.IsNullOrEmpty(type))
        {
            MonoBehaviour mb = mono.GetComponent(type) as MonoBehaviour;
            return mb;
            //if (mono.GetType() == System.Type.GetType(type))
            //{
            //    return mono;
            //}
            //else
            //{

            //}
        }
        return mono;
    }
    public MonoBehaviour[] GetComps(string type)
    {
        List<MonoBehaviour> mos = new List<MonoBehaviour>();
        foreach (var mono in mComDic.Values)
        {
            if (mono.GetType() == System.Type.GetType(type))
                mos.Add(mono);
        }
        return mos.ToArray();
    }
    public MonoBehaviour GetComp(string name)
    {
        if (mComDic.ContainsKey(name))
        {
            MonoBehaviour mono = mComDic[name];
            return mono;
        }
        //Debuger.LogError(string.Format(this.name + ".GetComp({0})不存在", name));
        return null;
    }
    
    public T GetComp<T>(string name) where T : MonoBehaviour
    {
        MonoBehaviour comp = null;
        if (mComDic.TryGetValue(name, out comp))
        {
            return comp as T;
        }
        return default(T);
    }
    public GameObject GetObj(string name)
    {
        MonoBehaviour mono = GetComp<MonoBehaviour>(name);
        if (mono != null)
        {
            return mono.gameObject;
        }
        return null;
    }
    public Transform GetTransform(string name)
    {
        MonoBehaviour mono = GetComp<MonoBehaviour>(name);
        if (mono != null)
        {
            return mono.transform;
        }
        return null;
    }

    public Image GetImage(string name)
    {
        MonoBehaviour mono = GetComp<MonoBehaviour>(name);
        if (mono != null)
        {
            return mono.GetComponent<Image>();
        }
        return null;
    }
    public Text GetTextBox(string name)
    {
        MonoBehaviour mono = GetComp<MonoBehaviour>(name);
        if (mono != null)
        {
            return mono.GetComponent<Text>();
        }
        return null;
    }
    public Camera GetCamera(string name)
    {
        MonoBehaviour mono = GetComp<MonoBehaviour>(name);
        if (mono != null)
        {
            return mono.GetComponent<Camera>();
        }
        return null;
    }
    public void SetText(string name,string text)
    {
        Text tb = this.GetComp<Text>(name);
        if (tb != null)
        {
            tb.text = text;
        }
    }
    public void SetVisible(string name,bool visible)
    {
        GameObject go = GetObj(name);
        if (go != null)
        {
            go.SetActive(visible);
        }
    }
    protected void Dispose()
    {
        this.mComDic.Clear();
        this.mComDic = null;
    }
    public virtual bool Visible
    {
        get
        {
            return this.gameObject.activeInHierarchy;
        }
        set
        {
            //visible = value;
            if (this.gameObject.activeInHierarchy == value) return;
            this.gameObject.SetActive(value);
        }
    }
    //public object[] CallLua(string sFunc, params object[] args)
    //{
    //    return XLua.LuaManager.CallLuaFunction(sFunc, args);
    //}

}
