using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class SRPSetting : MonoBehaviour
{
    public Camera mainCamera;
    public Camera uiCamera;
    public CanvasScaler CanvasScaler;
    public LayerMask[] Layers;
    public LayerMask RoleLayer;
    public PostProcessLayer PostLayer;
    public PostProcessVolume PostVolume;
    public PostProcessProfile PostProfile;
    public PlanarShadowPass ShadowPass;
    public Material radialBlurMat;
    public Material DissolveMat; 
    public Shader[] shaders;
    public Material[] materials;
    public Material GrayMat;
    public Material ColorAdjustMat;
    public Material DissolveMatInst
    {
        get
        {
            if (DissolveMat != null)
                return new Material(DissolveMat);
            Debug.LogError("DissolveMat == null");
            return null;
        }
    }
    public XRayPass XRayPass;
    public OutlinePass OutlinePass;
    public Transform maskLayer;
    public Canvas maskWaiting;
    public static SRPSetting Inst;
    public void Awake()
    {
        Inst = this;
    }
    public static bool PostVisible
    {
        get
        {
            return Inst.PostLayer.enabled;
        }
        set
        {
            Inst.PostLayer.enabled = value;
        }
    }
    public static bool BloomVisible
    {
        get
        {
            return Inst.PostProfile.GetSetting<Bloom>().active;
        }
        set
        {
            Inst.PostProfile.GetSetting<Bloom>().active = value;
        }
    }
    public static bool EdgeDetectVisible
    {
        get
        {
            return Inst.PostProfile.GetSetting<EdgeDetectPostProcessing>().active;
        }
        set
        {
            Inst.PostProfile.GetSetting<EdgeDetectPostProcessing>().active = value;
        }
    }
    public static bool RadialBlurVisible
    {
        get
        {
            return radialBlur.active;
        }
        set
        {
            radialBlur.active = value;
        }
    }
    public static Bloom bloom
    {
        get
        {
            return Inst.PostProfile.GetSetting<Bloom>();
        }
    }
    public static RadialBlurPostProcessing radialBlur
    {
        get
        {
            return Inst.PostProfile.GetSetting<RadialBlurPostProcessing>();
        }
    }
    public static ColorAdjustPostProcessing ColorAdjust
    {
        get
        {
            return Inst.PostProfile.GetSetting<ColorAdjustPostProcessing>();
        }
    }
    public static bool GrayPost
    {
        get
        {
            return ColorAdjust.active;
        }
        set
        {
            ColorAdjust.active = value;
            ColorAdjust.saturation.value = value ? 0f : 1f;
        }
    }
    public static int blurFactor
    {
        get
        {
            return radialBlur.blurFactor.value;
        }
        set
        {
            radialBlur.blurFactor.value = value;
        }
    }
    public static void SetShadow(bool b)
    {
        Inst.ShadowPass.IsOpen = b;
    }
    public static void SetXRay(bool b)
    {
        Inst.XRayPass.IsOpen = b;
    }
    public static void SetOutline(bool b)
    {
        Inst.OutlinePass.IsOpen = b;
    }
    public static void SetLayerIdx(GameObject go,int idx)
    {
        if(idx >= 0 && idx < Inst.Layers.Length)
        {
            SetLayer(go,Inst.Layers[idx]);
        }
    }
    public static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        var t = go.transform;

        for (int i = 0, imax = t.childCount; i < imax; ++i)
        {
            var child = t.GetChild(i);
            SetLayer(child.gameObject, layer);
        }
    }
    public static void SetRenderingLayerMask(GameObject go, int layer,bool v)
    {
        Renderer[] renders = go.GetComponentsInChildren<Renderer>();
        uint layerValue = (uint)(1 << layer);
        if(!v)
            layerValue = (~layerValue);
        for (int i=0;i<renders.Length;i++)
        {
            if (v)
            {
                renders[i].renderingLayerMask |= layerValue;
            }
            else
            {
                renders[i].renderingLayerMask &= layerValue;
            }
        }
        
    }
    public static void SetGrays(GameObject go,bool v)
    {
        MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
        for(int i=0;i< graphics.Length;i++)
        {
            SetGray(graphics[i], v);
        }
    }
    public static void SetGray(MaskableGraphic graphic,bool v)
    {
        if (graphic == null)
            return;
        if (graphic is Text)
            return;
        if (v && graphic.material == Inst.GrayMat)
            return;
        if (!v && graphic.material == graphic.defaultMaterial)
            return;
        graphic.material = v ? Inst.GrayMat : graphic.defaultMaterial;
    }
}
