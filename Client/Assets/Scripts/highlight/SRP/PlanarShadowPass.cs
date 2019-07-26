using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Rendering;


public class PlanarShadowPass : MonoBehaviour, IAfterOpaquePass
{
    public int renderingLayerMask =2;
    //public Color _SColor;
    //public float _High;
    //public float _ShadowFalloff;
    //public Vector4 _LightDir;
    public Material mat;
    public bool IsOpen = true;
    private PlanarShadowPassImpl m_PlanarShadowPass;

    public ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle, RenderTargetHandle depthHandle)
    {
        if (m_PlanarShadowPass == null) m_PlanarShadowPass = new PlanarShadowPassImpl(colorHandle, this);
        return m_PlanarShadowPass;
    }
}


public class PlanarShadowPassImpl : ScriptableRenderPass
{
    //const string k_PlanarShadowPassTag = "PlanarShadowPass";
    private RenderTargetHandle m_ColorHandle;
    private FilterRenderersSettings m_PerObjectFilterSettings;
    private PlanarShadowPass m_Pass;
    public PlanarShadowPassImpl(RenderTargetHandle colorHandle, PlanarShadowPass pass)
    {
        m_Pass = pass;
        RegisterShaderPassName("LightweightForward");//LightweightForward
        m_ColorHandle = colorHandle;
        m_PerObjectFilterSettings = new FilterRenderersSettings(true)
        {
            // Render all opaque objects
            renderQueueRange = RenderQueueRange.all,
            // Filter further by any renderer tagged as per-object blur
            renderingLayerMask = (uint)1 << (pass.renderingLayerMask - 1),
        };
    }
    public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (m_Pass == null || m_Pass.mat == null || !m_Pass.IsOpen)
            return;
        m_PerObjectFilterSettings.renderingLayerMask = (uint)1 << (m_Pass.renderingLayerMask - 1);
        //var drawSettings = new DrawRendererSettings(renderingData.cameraData.camera, new ShaderPassName("Outline"));
        var camera = renderingData.cameraData.camera;
        // We want the same rendering result as the main opaque render
        var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
        var drawSettings = CreateDrawRendererSettings(camera, sortFlags, RendererConfiguration.None, renderingData.supportsDynamicBatching);
        drawSettings.SetOverrideMaterial(m_Pass.mat, 0);
        context.DrawRenderers(renderingData.cullResults.visibleRenderers, ref drawSettings, m_PerObjectFilterSettings);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {

    }
}
