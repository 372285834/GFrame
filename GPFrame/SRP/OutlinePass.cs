using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;
using UnityEngine.Rendering;


public class OutlinePass : MonoBehaviour, IAfterOpaquePass
{
    public int renderingLayerMask = 4;
    public bool IsOpen = true;
    private OutlinePassImpl m_OutlinePass;

    public ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle, RenderTargetHandle depthHandle)
    {
        if (m_OutlinePass == null) m_OutlinePass = new OutlinePassImpl(colorHandle, this);
        return m_OutlinePass;
    }
}


public class OutlinePassImpl : ScriptableRenderPass
{
    //const string k_PlanarShadowPassTag = "PlanarShadowPass";
    private RenderTargetHandle m_ColorHandle;
    private FilterRenderersSettings m_PerObjectFilterSettings;
    private OutlinePass m_Pass;
    public OutlinePassImpl(RenderTargetHandle colorHandle, OutlinePass pass)
    {
        m_Pass = pass;
        // RegisterShaderPassName("NPR_Transparent");//LightweightForward
        RegisterShaderPassName("OUTLINE");//LightweightForward

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
        if (m_Pass == null || !m_Pass.IsOpen)
            return;
        m_PerObjectFilterSettings.renderingLayerMask = (uint)1 << (m_Pass.renderingLayerMask - 1);
        //var drawSettings = new DrawRendererSettings(renderingData.cameraData.camera, new ShaderPassName("Outline"));
        var camera = renderingData.cameraData.camera;
        // We want the same rendering result as the main opaque render
        var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
        var drawSettings = CreateDrawRendererSettings(camera, sortFlags, RendererConfiguration.None, renderingData.supportsDynamicBatching);
        //drawSettings.SetOverrideMaterial(m_Pass.mat, 0);
        context.DrawRenderers(renderingData.cullResults.visibleRenderers, ref drawSettings, m_PerObjectFilterSettings);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {

    }
}
