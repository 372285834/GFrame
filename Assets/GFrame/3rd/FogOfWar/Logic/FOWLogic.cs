using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 说明：战争迷雾表现逻辑，实现所有FOW逻辑表现接口以及与FOWSystem的对接
/// 
/// @by wsh 2017-05-19
/// </summary>

public class FOWLogic : Singleton<FOWLogic>
{
    private MapFOWRender m_mapFOWRender;
    public FOWSystem.Setting mSetting;
    // 视野体
    private List<IFOWRevealer> m_revealers = new List<IFOWRevealer>();
    // 渲染器
    private List<FOWRender> m_renders = new List<FOWRender>();
    public GameObject prefab;
    public override void Init()
    {
        base.Init();
        m_revealers.Clear();
        m_renders.Clear();
        mSetting = new FOWSystem.Setting();
    }
    public void Startup(GameObject _prefab,Transform parent)
    {
        if(_prefab == null)
        {
            Debug.LogError("fow no prefab");
            return;
        }
        FOWSystem.Instance.Startup(mSetting);
        prefab = _prefab;
        if (parent == null)
            parent = FOWSystem.Instance.transform;
        m_mapFOWRender = new MapFOWRender(parent);
    }
    public override void Dispose()
    {

        for (int i = 0; i < m_revealers.Count; i++)
        {
            IFOWRevealer revealer = m_revealers[i];
            if (revealer != null)
            {
                revealer.Release();
            }
        }
        m_revealers.Clear();

        for (int i = 0; i < m_renders.Count; i++)
        {
            FOWRender render = m_renders[i];
            if (render != null)
            {
                render.enabled = false;
                UnityEngine.Object.Destroy(render.gameObject);
            }
        }
        m_renders.Clear();

        m_mapFOWRender = null;
        FOWSystem.Instance.DestroySelf();
    }

    public void AddCharactor(int charaID,GameObject go,float radius)
    {
        var irevealer = m_revealers.Find(x => x.charaID() == charaID);
        if (irevealer != null)
            irevealer.SetValid(true);
        else
        {
            FOWCharactorRevealer revealer = FOWCharactorRevealer.Get();
            revealer.InitInfo(charaID,go, radius);
            FOWSystem.AddRevealer(revealer);
            m_revealers.Add(revealer);
        }
    }
    public void RemoveCharactor(int charaID)
    {
        var revealer = m_revealers.Find(x => x.charaID() == charaID);
        if (revealer != null)
            revealer.SetValid(false);
    }
    public FOWRender CreateRender(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }

        FOWRender render = null;
        // TODO：实际项目中，从这里的资源管理类加载预设
        // 为了简单，这里直接从Resource加载
        if (prefab != null)
        {
            GameObject mesh = GameObject.Instantiate(prefab) as GameObject;
            if (mesh != null)
            {
                mesh.transform.parent = parent;
                render = mesh.gameObject.AddComponent<FOWRender>();
            }
        }

        if (render != null)
        {
            m_renders.Add(render);
        }
        return render;
    }

    private void ActivateRender(FOWRender render, bool active)
    {
        if (render != null)
        {
            render.Activate(active);
        }
    }

    public void Update(int deltaMS)
    {
        // 说明：每个游戏帧更新，这里不做时间限制，实测对游戏帧率优化微乎其微
        UpdateRenders();
        UpdateRevealers(deltaMS);
    }

    protected void UpdateRenders()
    {
        for (int i = 0; i < m_renders.Count; i++)
        {
            ActivateRender(m_renders[i], FOWSystem.Instance.setting.enableRender);
        }
    }

    protected void UpdateRevealers(int deltaMS)
    {
        for (int i = m_revealers.Count - 1; i >= 0; i--)
        {
            IFOWRevealer revealer = m_revealers[i];
            revealer.Update(deltaMS);
            if (!revealer.IsValid())
            {
                m_revealers.RemoveAt(i);
                FOWSystem.RemoveRevealer(revealer);
                revealer.Release();
            }
        }
    }

    public void AddTempRevealer(Vector3 position, float radius, int leftMS)
    {
        if (leftMS <= 0)
        {
            return;
        }

        FOWTempRevealer tmpRevealer = FOWTempRevealer.Get();
        tmpRevealer.InitInfo(position, radius, leftMS);
        FOWSystem.AddRevealer(tmpRevealer);
        m_revealers.Add(tmpRevealer);
    }
}

