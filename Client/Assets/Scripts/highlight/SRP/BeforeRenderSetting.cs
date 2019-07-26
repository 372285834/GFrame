using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

public class BeforeRenderSetting : MonoBehaviour, IBeforeCameraRender
{

    public void ExecuteBeforeCameraRender(LightweightRenderPipeline pipelineInstance, ScriptableRenderContext context, Camera camera)
    {
        
    }
}
