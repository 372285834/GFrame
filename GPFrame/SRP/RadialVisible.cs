using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialVisible : MonoBehaviour
{
    public int blurFactor = 40;
    void OnEnable()
    {
        SRPSetting.RadialBlurVisible = true;
    }
    void Update()
    {
        SRPSetting.blurFactor = blurFactor;
    }
    void OnDisable()
    {
        SRPSetting.RadialBlurVisible = false;
    }
}
