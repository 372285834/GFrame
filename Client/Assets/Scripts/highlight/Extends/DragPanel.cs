using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace highlight
{
    public class DragPanel : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public RectTransform rectTransform;
	    public Vector2 originalLocalPointerPosition;
        public Vector3 originalPanelLocalPosition;
        public RectTransform panelRectTransform;
        public RectTransform parentRectTransform;
        public RectTransform clampRect;
        public bool AutoReset = false;
       // [XLua.LuaCallCSharp]
        //[XLua.CSharpCallLua]
        public AcHandler<bool> AcDrag;
       // [XLua.LuaCallCSharp]
       // [XLua.CSharpCallLua]
        public AcHandler<PointerEventData> AcDragUpdate;
        public bool IsClamp = true;
	    void Awake () {
            this.rectTransform = this.transform as RectTransform;
            panelRectTransform = transform.parent as RectTransform;
		    parentRectTransform = panelRectTransform.parent as RectTransform;
	    }

        public void OnPointerDown(PointerEventData data)
        {
            originalPanelLocalPosition = panelRectTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
        }

        public void OnDrag (PointerEventData data) {
		    if (panelRectTransform == null || parentRectTransform == null)
			    return;
		    
		    Vector2 localPointerPosition;
		    if (RectTransformUtility.ScreenPointToLocalPointInRectangle (parentRectTransform, data.position, data.pressEventCamera, out localPointerPosition)) {
			    Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
			    panelRectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
		    }
		
		    ClampToWindow (data.pressEventCamera);
            if (AcDragUpdate != null)
                AcDragUpdate(data);
	    }
        public void OnBeginDrag(PointerEventData data)
        {
            //originalPanelLocalPosition = panelRectTransform.localPosition;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
            if (AcDrag != null)
                AcDrag(true);
        }
        public void OnEndDrag(PointerEventData data)
        {
            if (AcDrag != null)
                AcDrag(false);
            if (AutoReset)
                panelRectTransform.localPosition = originalPanelLocalPosition;
        }
        Vector3[] corners;
	    // Clamp panel to area of parent
	    void ClampToWindow (Camera ca) {
            if (!IsClamp)
                return;
            Rect rt = parentRectTransform.rect;
            if(clampRect != null)
            {
                if (corners == null)
                    corners = new Vector3[4];
                clampRect.GetWorldCorners(corners);
                Vector3 sPos = ca.WorldToScreenPoint(corners[0]);
                Vector2 lt;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, sPos, ca, out lt);
                rt = new Rect(lt.x, lt.y, clampRect.rect.width, clampRect.rect.height);
            }
            Vector3 pos = panelRectTransform.localPosition;
		    
		    Vector3 minPosition = rt.min - panelRectTransform.rect.min;
		    Vector3 maxPosition = rt.max - panelRectTransform.rect.max;
		
		    pos.x = Mathf.Clamp (panelRectTransform.localPosition.x, minPosition.x, maxPosition.x);
		    pos.y = Mathf.Clamp (panelRectTransform.localPosition.y, minPosition.y, maxPosition.y);
		
		    panelRectTransform.localPosition = pos;
	    }
    }
}
