using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 说明：角色视野
/// 
/// @by wsh 2017-05-20
/// </summary>

public class FOWCharactorRevealer : FOWRevealer
{
    protected static HashSet<int> m_allChara = new HashSet<int>();
    protected Transform transform;
    public FOWCharactorRevealer()
    {
    }

    static public new FOWCharactorRevealer Get()
    {
        return ClassObjPool<FOWCharactorRevealer>.Get();
    }

    static public bool Contains(int charaID)
    {
        return m_allChara.Contains(charaID);
    }

    public override void OnInit()
    {
        base.OnInit();
        m_charaID = 0;
    }

    public override void OnRelease()
    {
        m_allChara.Remove(m_charaID);

        base.OnRelease();
    }

    public void InitInfo(int charaID, GameObject go, float radius)
    {
        m_charaID = charaID;
        transform = go.transform;
        m_radius = radius;
        m_allChara.Add(m_charaID);
        m_isValid = true;
        Update(0);
    }
    
    public override void Update(int deltaMS)
    {
        if (this.transform == null)
        {
            m_isValid = false;
            return;
        }
        m_position = this.transform.position;
    }
}