using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


[PostProcess(typeof(ColorAdjustRenderer), PostProcessEvent.AfterStack, "Custom/ColorAdjust")]
public class ColorAdjustPostProcessing : PostProcessEffectSettings
{
    //通过Range控制可以输入的参数的范围

    [Range(0.0f, 3.0f)]
    public FloatParameter brightness = new FloatParameter() { value = 1.0f };//亮度

    [Range(0.0f, 3.0f)]
    public FloatParameter contrast = new FloatParameter() { value = 1.0f };  //对比度

    [Range(0.0f, 3.0f)]
    public FloatParameter saturation = new FloatParameter() { value = 1.0f };//饱和度
}

//--------------------------------------------------------------------------------------------------------------------------------

public class ColorAdjustRenderer : PostProcessEffectRenderer<ColorAdjustPostProcessing>
{
    Material mat;
    public ColorAdjustRenderer()
    {
        if (SRPSetting.Inst == null)
            return;
        mat = SRPSetting.Inst.ColorAdjustMat;
    }

    public override void Render(PostProcessRenderContext context)
    {
        if (settings == null || mat == null)
            return;
        CommandBuffer cmd = context.command;
        mat.SetFloat("_Brightness", settings.brightness);
        mat.SetFloat("_Saturation", settings.saturation);
        mat.SetFloat("_Contrast", settings.contrast);
        cmd.Blit(context.source, context.destination, mat, 0);
    }
}
