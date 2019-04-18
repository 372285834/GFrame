using UnityEngine;

/// <summary>
/// 说明：FOW表现层渲染脚本
/// 
/// @by wsh 2017-05-20
/// </summary>

public class FOWRender : MonoBehaviour
{
    Material mMat;

    void Start()
    {
        if (mMat == null)
        {
            MeshRenderer render = GetComponentInChildren<MeshRenderer>();
            if (render != null)
            {
                mMat = render.material;
            }
        }

        if (mMat == null)
        {
            enabled = false;
            return;
        }
    }

    public void Activate(bool active)
    {
        gameObject.SetActive(active);
    }

    public bool IsActive
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

    void OnWillRenderObject()
    {
        if (mMat != null && FOWSystem.Instance.texture != null)
        {
            mMat.SetTexture("_MainTex", FOWSystem.Instance.texture);
            mMat.SetFloat("_BlendFactor", FOWSystem.Instance.blendFactor);
            FOWSystem.Setting setting = FOWSystem.Instance.setting;
            if (setting.enableFog)
            {
                mMat.SetColor("_Unexplored", setting.unexploredColor);
            }
            else
            {
                mMat.SetColor("_Unexplored", setting.exploredColor);
            }
            mMat.SetColor("_Explored", setting.exploredColor);
        }
    }
}
