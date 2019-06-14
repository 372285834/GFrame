using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITabItem : IUIItem, IPointerClickHandler, IPointerDownHandler,IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum TabState
    {
        Normal,
        Highlighted,
        Pressed,
        Disable,
    }
    TabState _state = TabState.Normal;
    public TabState state
    {
        get
        {
            if (!this.interactable)
                return TabState.Disable;
            return _state;
        }
    }
    public Image Checkmark;
    public List<TabColorComponent> TabList;
    private bool isPointerInside { get; set; }
    private bool isPointerDown { get; set; }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
            return;
        this.SetSelect();
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        if (!interactable)
            return;
        changeState(TabState.Pressed);
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (!interactable)
            return;
        changeState(isPointerInside?TabState.Highlighted: TabState.Normal);
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        if (!interactable)
            return;
        changeState(TabState.Highlighted);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        if (!interactable)
            return;
        changeState(isPointerDown ? TabState.Highlighted : TabState.Normal);
    }
    void changeState(TabState _state)
    {
        this._state = _state;
        this.ChangeColor(false);
    }
    public override void ChangeColor(bool instant = true)
    {
        if (Checkmark != null && Checkmark.gameObject.activeSelf != IsSelected)
            Checkmark.gameObject.SetActive(IsSelected);
        if (TabList == null)
            return;
        for (int i = 0; i < TabList.Count; i++)
        {
            TabList[i].Init(this, instant);
        }
    }
    public override void SerializeFieldInfo()
    {
        if (TabList == null)
            TabList = new List<TabColorComponent>();
        TabList.Clear();
        TabColorComponent[] tabs = this.gameObject.GetComponentsInChildren<TabColorComponent>();
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].gameObject.GetCompInParent<UITabItem>() == this)
                TabList.Add(tabs[i]);
        }
        base.SerializeFieldInfo();
    }
}