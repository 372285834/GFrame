using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


[PostProcess(typeof(RadialBlurRenderer), PostProcessEvent.AfterStack, "Custom/RadialBlur")]
public class RadialBlurPostProcessing : PostProcessEffectSettings
{
    //模糊程度，不能过高
    [Range(0, 50), Tooltip("blurFactor")]
    public IntParameter blurFactor = new IntParameter { value = 10 };
    [Range(0.0f, 1.0f)]
    public FloatParameter lerpFactor = new FloatParameter { value = 0.5f };
    //降低分辨率
    public IntParameter downSampleFactor = new IntParameter { value = 2 };
    //模糊中心（0-1）屏幕空间，默认为中心点
    [Tooltip("blurCenter")]
    public Vector2Parameter blurCenter = new Vector2Parameter { value = new Vector2(0.5f, 0.5f) };
}

//--------------------------------------------------------------------------------------------------------------------------------

public class RadialBlurRenderer : PostProcessEffectRenderer<RadialBlurPostProcessing>
{
    Material mat;
    int m_BlurTemp1;
    int m_BlurTemp2;
    public RadialBlurRenderer()
    {
        if (SRPSetting.Inst == null)
            return;
        mat = SRPSetting.Inst.radialBlurMat;
        m_BlurTemp1 = Shader.PropertyToID("_Temp1");
        m_BlurTemp2 = Shader.PropertyToID("_Temp2");
    }

    public override void Render(PostProcessRenderContext context)
    {
        if (settings == null || mat == null)
            return;
        CommandBuffer cmd = context.command;

        int factor = settings.downSampleFactor;
        if(factor <= 1)
        {
            cmd.Blit(context.source, context.destination, mat, 0);
            return;
        }
        // Create our temp working buffers, work at quarter size
        context.GetScreenSpaceTemporaryRT(cmd, m_BlurTemp1, 0, context.sourceFormat,
        RenderTextureReadWrite.Default, FilterMode.Bilinear, context.width / factor, context.height / factor);
        context.GetScreenSpaceTemporaryRT(cmd, m_BlurTemp2, 0, context.sourceFormat,
        RenderTextureReadWrite.Default, FilterMode.Bilinear, context.width / factor, context.height / factor);
        cmd.Blit(context.source, m_BlurTemp1);
        // Copy all values about our brightness and inside our mask to a temp buffer
        //使用降低分辨率的rt进行模糊:pass0
        mat.SetFloat("_BlurFactor", settings.blurFactor * 0.001f);
        mat.SetVector("_BlurCenter", settings.blurCenter);
        cmd.Blit(m_BlurTemp1, m_BlurTemp2, mat,0);

        // Blit the blurred brightness back into the color buffer, optionally increasing the brightness
        //使用rt2和原始图像lerp:pass1
        cmd.SetGlobalTexture("_BlurTex", m_BlurTemp2);
       // mat.SetTexture("_BlurTex", m_BlurTemp2);
        mat.SetFloat("_LerpFactor", settings.lerpFactor);
        cmd.Blit(context.source, context.destination, mat, 1);

        // Cleanup
        cmd.ReleaseTemporaryRT(m_BlurTemp1);
        cmd.ReleaseTemporaryRT(m_BlurTemp2);
    }
}
