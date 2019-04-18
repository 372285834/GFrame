using highlight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TabColorComponent : MonoBehaviour, ISerializeField
{
    public enum eColorType
    {
        None,
        Text1,
        ShopTab1,
        ShopTab2,
        Image1,
        Mask1,
    }
    public eColorType eType = eColorType.None;
    public Graphic target;
    public ColorBlock colors = ColorBlock.defaultColorBlock;

    [Tooltip("选中颜色,a==0时 不支持选种状态")]
    public Color SelectColor = new Color32(0, 0, 0, 0);
    public void SerializeFieldInfo()
    {
        UITabItem item = gameObject.GetCompInParent<UITabItem>();
        if (item != null)
            item.SerializeFieldInfo();
    }
    void OnEnable()
    {
        Init(mItem,true);
    }
    //void OnDisable()
    //{
    //    StartColorTween(Color.white, true);
    //}
    UITabItem mItem;
    public void Init(UITabItem item, bool instant = true)
    {
        if (target == null)
            return;
        mItem = item;
        Color co = colors.normalColor;
        if (item == null)
        {
            SetColor(co);
            return;
        }
        switch (item.state)
        {
            case UITabItem.TabState.Normal:
                if (item.IsSelected && SelectColor.a > 0f)
                    co = SelectColor;
                else
                    co = colors.normalColor;
                break;
            case UITabItem.TabState.Highlighted:
                co = colors.highlightedColor;
                break;
            case UITabItem.TabState.Pressed:
                co = colors.pressedColor;
                break;
            case UITabItem.TabState.Disable:
                co = colors.disabledColor;
                break;
            default:
                break;
        }
        StartColorTween(co * colors.colorMultiplier, instant);
    }
    public void StartColorTween(Color targetColor, bool instant = true)
    {
        if (target == null)
            return;
        float f = instant ? 0f : colors.fadeDuration;
        if (f > 0f)
            target.CrossFadeColor(targetColor, f, true, true);
        else
            SetColor(targetColor);
        
    }
    public void SetColor(Color co)
    {
        if (target == null)
            return;
        target.canvasRenderer.SetColor(co);
    }
}
