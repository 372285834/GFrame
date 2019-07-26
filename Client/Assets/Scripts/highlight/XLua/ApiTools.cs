using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using highlight;
using System;

public static class ApiTools
{
    //public class ApiData
    //{
    //    public string key;
    //    public Action<object[]> fun;
    //    public int parmNum = 1;
    //    public ApiData(string _key, Action<object[]> _fun,int num)
    //    {
    //        this.key = _key;
    //        this.fun = _fun;
    //        this.parmNum = num;
    //    }
    //    public bool CheckParam(object lua1, object lua2, object lua3)
    //    {
    //        //if(parmNum == )
    //        return true;
    //    }
    //}
    public delegate object ApiFun(object[] go);
    static Dictionary<string, ApiFun> mDic = new Dictionary<string, ApiFun>
    {
        {"AddSceneData",AddSceneData},
    };

    public static object SetApi(string key, object lua1)
    {
        return SetApi(key, lua1, null, null);
    }
    public static object SetApi(string key, object lua1, object lua2)
    {
        return SetApi(key, lua1, lua2, null);
    }
    public static object SetApi(string key, object lua1, object lua2, object lua3)
    {
        try
        {
            ApiFun fun = null;
            if (mDic.TryGetValue(key, out fun))
            {
                return fun(new object[] { lua1, lua2, lua3 });
            }
        }
        catch(Exception e)
        {
            Debug.LogError(key + "\n" + e.Message + "\n" + e.ToString());
        }
        return null;
    }
    static object AddSceneData(object[] args)
    {
        if (args.Length < 2)
            return null;
        //BuglyInit.AddSceneData(args[0].ToString(), args[1].ToString());
        return null;
    }

    public static TrailRenderer GetTrailRenderer(GameObject go)
    {
        return go.GetComponent<TrailRenderer>();
    }
    public static void ClearTrailRenderer(TrailRenderer tr)
    {
        if (tr != null)
            tr.Clear();
    }

    public static GameObject currentSelectedGameObject
    {
        get { return UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject; }
    }
    
    public static void GetPosition(Component obj, out float x, out float y, out float z)
    {
        Vector3 pos = obj.transform.position;
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }
    public static void GetPosition(GameObject obj, out float x, out float y, out float z)
    {
        Vector3 pos = obj.transform.position;
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }
    public static void SetPosition(GameObject go, float x, float y, float z)
    {
        go.transform.position = new Vector3(x, y, z);
    }
    public static void SetRotation(GameObject go, float x, float y, float z, float w)
    {
        go.transform.rotation = new Quaternion(x, y, z, w);
    }
    public static void SetLocalPosition(GameObject go, float x, float y, float z)
    {
        go.transform.localPosition = new Vector3(x, y, z);
    }
    public static void SetLocalScale(GameObject go, float x, float y, float z)
    {
        go.transform.localScale = new Vector3(x, y, z);
    }
    public static void SetLocalRotation(GameObject go, float x, float y, float z, float w)
    {
        go.transform.localRotation = new Quaternion(x, y, z, w);
    }
    public static void Identity(Component c)
    {
        c.transform.localScale = Vector3.one;
        c.transform.localRotation = Quaternion.identity;
        c.transform.localPosition = Vector3.zero;
    }
    public static void Identity(GameObject go)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
    }
    public static void SetPosComp(Component comp, float x, float y, float z)
    {
        comp.transform.position = new Vector3(x, y, z);
    }
    public static void SetLocalPosComp(Component comp, float x, float y, float z)
    {
        comp.transform.localPosition = new Vector3(x, y, z);
    }
    public static void SetLocalScaleComp(Component comp, float x, float y, float z)
    {
        comp.transform.localScale = new Vector3(x, y, z);
    }
    public static void SetBoxColliderSize(GameObject go, float x, float y, float z)
    {
        BoxCollider bc = go.AddComp<BoxCollider>();
        bc.size = new Vector3(x,y,z);
    }
    public static void SetBoxColliderCenterAndSize(GameObject go, float cx, float cy, float cz, float sx, float sy, float sz)
    {
        BoxCollider bc = go.AddComp<BoxCollider>();
        bc.center = new Vector3(cx, cy, cz);
        bc.size = new Vector3(sx, sy, sz);
    }

    static public GameObject Instantiate(GameObject prefab)
    {
        GameObject go;
        if (prefab != null)
        {
            go = GameObject.Instantiate(prefab) as GameObject;
            go.name = prefab.name;
        }
        else
        {
            go = new GameObject();
        }
        return go;
    }
    public static void SetActive(UnityEngine.Object obj, bool state)
    {
        if (obj == null)
            return;
        GameObject go = obj is GameObject ? obj as GameObject : (obj as Component).gameObject;
        if (go.activeSelf != state)
            go.SetActive(state);
    }
    public static int GetPlateForm()
    {
        return (int)Application.platform;
    }
    public static void IdentityPanel (Transform trans)
    {
        RectTransform rect = trans as RectTransform;
        if (trans == null)
            return;
        rect.sizeDelta = Vector2.zero;
        Identity(trans);
    }
    public static GameObject Instantiate(GameObject source,Transform parent=null)
    {
        GameObject go = null;
        if (parent == null)
            parent = source.transform.parent;
        go = GameObject.Instantiate(source, parent);
        return go;
    }
    static public void SetLayer(GameObject aGo, int aLayer)
    {
        if (aGo.layer == aLayer)
            return;
        aGo.layer = aLayer;
        Renderer[] rends = aGo.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i].gameObject.layer != aLayer)
                rends[i].gameObject.layer = aLayer;
        }
    }

    public static void BeginSample(string str)
    {
        // UWAEngine.PushSample(str);
        UnityEngine.Profiling.Profiler.BeginSample(str);
    }
    public static void EndSample()
    {
        // UWAEngine.PopSample();
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
