using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderBox : MonoBehaviour
{
    public enum eStateType
    {
        Normal,
        Alpha,
        Dissolve,
        StopDissolve,
    }
    public eStateType eType = eStateType.Normal;
    public float progress;
    public Renderer mRender;
    public Material[] dissMat;
    private Material[] sMat;
    private int layer;
    private bool canDissolve = false;
    private void Awake()
    {
        layer = this.gameObject.layer;
        if (mRender == null)
        {
            mRender = GetComponent<Renderer>();
            if (mRender == null)
                return;
            sMat = mRender.sharedMaterials;
            this.enabled = false;
        }
        canDissolve = mRender != null && sMat != null && mRender.enabled && sMat.Length > 0 && sMat[0] != null;
    }
    public float speed = 1f;
    public void DissolveStart(float p=0f)
    {
        if (!canDissolve)
            return;
        eType = eStateType.Dissolve;
        if (!this.enabled)
        {
            progress = p;
            if(SRPSetting.Inst != null)
            {
                if (dissMat == null)
                    dissMat = new Material[1] { SRPSetting.Inst.DissolveMatInst };
                dissMat[0].mainTexture = sMat[0].mainTexture;
                mRender.sharedMaterials = dissMat;
            }
            
            this.enabled = true;
        }
    }
    public void DissolveStop()
    {
        if (!canDissolve)
            return;
        eType = eStateType.StopDissolve;
           // mRender.enabled = true;
    }
    public void DissolveStopImp()
    {
        if (!canDissolve)
            return;
        eType = eStateType.Normal;
        this.enabled = false;
        progress = 0;
        mRender.sharedMaterials = sMat;
          //  mRender.enabled = true;
    }
    void Update()
    {
        if (!canDissolve)
            return;
        if (eType == eStateType.Dissolve)
        {
            if (progress < 1f)
                progress += Time.deltaTime * speed;
            //if(progress >= 1f && canDissolve)
           //     mRender.enabled = false;

        }
        else if (eType == eStateType.StopDissolve)
        {
            if (progress > 0f)
                progress -= Time.deltaTime * speed;
            if(progress <= 0f)
                this.DissolveStopImp();
        }
    }
    void OnWillRenderObject()
    {
        if (this.enabled && dissMat != null && dissMat[0] != null)
        {
            dissMat[0].SetFloat("_DissolveThreshold", progress);
        }
    }
    public void SetLayer(int _layer = 0)
    {
        if (_layer > 0)
        {
            this.gameObject.layer = _layer;
        }
        else
        {
            this.gameObject.layer = this.layer;
        }
    }
    private void OnDestroy()
    {
        if(dissMat != null && dissMat[0] != null)
        {
            GameObject.Destroy(dissMat[0]);
        }
        dissMat = null;
    }
    public static void SetLayer(GameObject go, int layer = 0)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            RenderBox rb = rends[i].AddComp<RenderBox>();
            rb.SetLayer(layer);
        }
    }
    public static void SetAlpha(GameObject go, float a, int layer=0)
    {
        if (go == null)
            return;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if (layer == 0 || LayerMaskExtensions.Contains(rends[i].renderingLayerMask, layer))
                RenderBox.SetPropertyBlockAlpha(rends[i], a);
        }
    }
    public static void SetColor(GameObject go, Color co, int layer=0)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if(layer == 0 || LayerMaskExtensions.Contains(rends[i].renderingLayerMask, layer))
                RenderBox.SetPropertyBlockColor(rends[i], co);
        }
    }
    //private static Shader _NCharacter;
    //public static Shader NCharacter { get { if (_NCharacter == null) _NCharacter = Shader.Find("Custom/NCharacter"); return _NCharacter; } }
    //private static Shader _NCharacter_Alpha;
    //public static Shader NCharacter_Alpha { get { if (_NCharacter_Alpha == null) _NCharacter_Alpha = Shader.Find("Custom/NCharacter_Alpha"); return _NCharacter_Alpha; } }

    static int _ColorID = Shader.PropertyToID("_Color");
    public static void SetPropertyBlockAlpha(Renderer render, float a)
    {
        if (render == null)
            return;
        bool isAlpha = a < 0.99999f;
        bool isOffShadow = a <= 0f;
        Material mat = render.material;
        render.shadowCastingMode = isOffShadow ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
        //if (mat.shader == NCharacter || mat.shader == NCharacter_Alpha)
        //{
        //    Shader sd = isAlpha ? NCharacter_Alpha : NCharacter;
        //    mat.shader = sd;
        //}
        //else
        {
            mat.SetFloat("_Surface", isAlpha ? 1f : 0f);

            if(isAlpha)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
               // mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                //mat.SetShaderPassEnabled("ShadowCaster", false);
            }
            else
            {
                mat.SetOverrideTag("RenderType", "");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
               // mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
                //mat.SetShaderPassEnabled("ShadowCaster", true);
            }
            if(isOffShadow)
            {
                mat.SetInt("_ZWrite", 0);
                mat.SetShaderPassEnabled("ShadowCaster", false);
            }
            else
            {
                mat.SetInt("_ZWrite", 1);
                mat.SetShaderPassEnabled("ShadowCaster", true);
            }
        }
        Color co = mat.GetColor(_ColorID);
        co.a = a;
        mat.SetColor(_ColorID, co);
    }
    public static void SetPropertyBlockColor(Renderer render, Color co)
    {
        if (render == null)
            return;
        render.material.SetColor(_ColorID, co);
    }


    public static void Dissolve(GameObject go, bool b,int layer = 0)
    {
        if (go == null)
            return;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if (layer == 0 || layer == rends[i].gameObject.layer)
            {
                RenderBox dissolve = rends[i].AddComp<RenderBox>();
                if (b)
                    dissolve.DissolveStart();
                else
                    dissolve.DissolveStop();
            }
        }
    }
}
