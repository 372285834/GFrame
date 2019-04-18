
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEngine.UI;

[CustomEditor(typeof(TabColorComponent))]
[ExecuteInEditMode]
[CanEditMultipleObjects]
//请继承Editor
public class TabColorComponentEditor : Editor
{
    TabColorComponent mScript;
    enum StateTool
    {
        Normal,
        Hight,
        Pressed,
        Disable,
        Select,
    }
    StateTool state = StateTool.Normal;
    //在这里方法中就可以绘制面板。
    public override void OnInspectorGUI()
    {
        mScript = (TabColorComponent)target;
        TabColorComponent.eColorType curType = mScript.eType;
        state = (StateTool)EditorGUILayout.EnumPopup("颜色测试",state);
        base.OnInspectorGUI();
        mScript.target = mScript.GetComponent<Graphic>();
        //if (mScript.target == null)
        //{
        //    if (mScript.target is Image)
        //        mScript.eType = TabColorComponent.eColorType.Image1;
        //}
        ColorBlock co = mScript.colors;
        if (curType != mScript.eType)
        {
            switch (mScript.eType)
            {
                case TabColorComponent.eColorType.Text1:
                    co.normalColor = new Color32(255, 255, 255, 255);
                    co.highlightedColor = new Color32(245, 245, 245, 255);
                    co.pressedColor = new Color32(200, 200, 200, 255);
                    co.disabledColor = new Color32(125, 125, 125, 255);
                    mScript.SelectColor = new Color32(200, 200, 200, 255);
                    break;
                case TabColorComponent.eColorType.ShopTab1:
                    co.normalColor = new Color32(251, 219, 184, 255);
                    co.highlightedColor = new Color32(255, 249, 242, 255);
                    co.pressedColor = new Color32(148, 107, 62, 255);
                    co.disabledColor = new Color32(125, 125, 125, 255);
                    mScript.SelectColor = new Color32(255, 192, 24, 255);
                    break;
                case TabColorComponent.eColorType.ShopTab2:
                    co.normalColor = new Color32(221, 185, 150, 255);
                    co.highlightedColor = new Color32(0, 0, 0, 255);
                    co.pressedColor = new Color32(108, 83, 58, 255);
                    co.disabledColor = new Color32(125, 125, 125, 255);
                    mScript.SelectColor = new Color32(0, 0, 0, 255);
                    break;
                case TabColorComponent.eColorType.Image1:
                    co.normalColor = new Color32(204, 204, 204, 255);
                    co.highlightedColor = new Color32(255, 255, 255, 255);
                    co.pressedColor = new Color32(255, 255, 255, 255);
                    co.disabledColor = new Color32(255, 255, 255, 255);
                    mScript.SelectColor = new Color32(255, 255, 255, 255);
                    break;
                case TabColorComponent.eColorType.Mask1:
                    co.normalColor = new Color32(0, 0, 0, 0);
                    co.highlightedColor = new Color32(255, 255, 255, 255);
                    co.pressedColor = new Color32(204, 204, 204, 255);
                    co.disabledColor = new Color32(0, 0, 0, 0);
                    mScript.SelectColor = new Color32(0, 0, 0, 0);
                    break;
                default:
                    break;
            }
            mScript.colors = co;
        }
        if(!Application.isPlaying)
        {
            switch (state)
            {
                case StateTool.Normal:
                    mScript.SetColor(co.normalColor);
                    break;
                case StateTool.Hight:
                    mScript.SetColor(co.highlightedColor);
                    break;
                case StateTool.Pressed:
                    mScript.SetColor(co.pressedColor);
                    break;
                case StateTool.Disable:
                    mScript.SetColor(co.disabledColor);
                    break;
                case StateTool.Select:
                    mScript.SetColor(mScript.SelectColor);
                    break;
                default:
                    break;

            }
        }
        
        EditorUtility.SetDirty(mScript.target);
    }
}