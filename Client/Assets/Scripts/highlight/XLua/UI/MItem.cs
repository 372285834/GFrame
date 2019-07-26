using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using highlight;
using UnityEngine.UI;
using System;
using XLua;
using System.Collections.Generic;

public class MItem : UITabItem//, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public LuaTable lua;
    public MUIFunction mUI;
    // Curve center offset 
    public override void OnAwake()
    {
        base.OnAwake();
        if (mUI == null)
            mUI = this.AddComp<MUIFunction>();
        mUI.InitComp();
        lua = UITools.GetUIData(this.gameObject);
        lua.SetInPath("mItem", this);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
       // EventSystem.current.SetSelectedGameObject(gameObject);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }
    public override void SerializeFieldInfo()
    {
            mUI = this.AddComp<MUIFunction>();
        base.SerializeFieldInfo();
    }
    public override void OnUpdate()
    {
            LuaDelegate.UpdateListItem.Call(this.mList, this);
    }
    public override void OnSelectItem()
    {
            LuaDelegate.SelectListItem.Call(this.mList, this);
    }
    void OnDestroy()
    {
        mUI = null;
        mList = null;
        if (lua != null)
        {
            lua.Dispose();
            lua = null;
        }
        //         mSelectImgList = null;
        //         mSelectTextList = null;
    }
    //public static string SelectName = "_SelectBg";
   // public static string SelectLabel = "_SelectLabel";
#if UNITY_EDITOR
  //  [LuaInterface.NoToLua]
  //  void Reset()
  //  {
       // mSelectImg = mSelectImg != null ? mSelectImg : transform.FindChild(SelectName).GetComponent<Image>();
       // mSelectText = mSelectText != null ? mSelectText : transform.FindChild(SelectLabel).GetComponent<Text>();
  //  }

#endif // if UNITY_EDITOR
}
