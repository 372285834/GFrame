using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using highlight;
//[RequireComponent(typeof(Graphic))]
public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,IPointerDownHandler,IPointerUpHandler
{
	public bool isDisable { get { return !isRote && !isMove; } }
    public bool isMoveX = false;
    public bool isMoveY = false;
    public bool isMoveZ = false;

    public bool isRote = true;
    public bool isReverse = false; // 是否反转数值
    public bool isMove { get { return isMoveX || isMoveY || isMoveZ; } }

    public GameObject m_DraggingIcon = null;

    public bool isLimit = false;
    public Vector2 mClampYRote = new Vector2(0f,360f);
    public Rect mClampRect = new Rect(0f, 0f, 100f, 100f);
    public float moveFovScale = 0.1f;
    public float vValueScale = 1; // 移动数值缩放，避免移动的过大
    public float Elasticity = 5;
    public Vector3 rotateAxis = new Vector3(0f, 1f, 0f);
    public bool IsDraging { get; private set; }
    public bool IsPointDown { get; private set; }
	public void OnBeginDrag(PointerEventData eventData)
	{
        if (m_DraggingIcon != null)
        {
            m_DraggingIcon.SetActive(true);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        IsDraging = false;
        IsPointDown = false;
        if (m_DraggingIcon != null)
        {
            m_DraggingIcon.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.IsPointDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.IsPointDown = false;
    }
    public static Transform CurDrageTarget;
    public Transform Target;
    public Vector3 scrollDelta;
    public void OnDrag(PointerEventData data)
    {
        if (isDisable)
            return;
        IsDraging = true;
        IsPointDown = true;
        if (Input.touchCount > 1)
        {
            return;
        }
        if (Target == null)
            return;
        CurDrageTarget = Target;
        //Debug.Log(data.delta);
        Transform transTarget = Target;
        if (isRote)
        {
            //Debug.Log (data.delta.x);
            float fEulerAdd = data.delta.x * this.vValueScale*0.1f;
            if (isLimit)
            {
                Vector3 euler = transTarget.eulerAngles;
                float fEulerTarget = euler.y + fEulerAdd;
                //|| fEulerTarget < vRoteYMin
                if (fEulerTarget > mClampYRote.y)
                {
                    fEulerAdd = mClampYRote.y - euler.y;
                    //fEulerTarget 
                    //fEulerAdd = fEulerAdd + 
                }
                else if (fEulerTarget < mClampYRote.x)
                {
                    fEulerAdd = mClampYRote.x - euler.y;
                }
            }
            if (isReverse)
                fEulerAdd = -fEulerAdd;
            transTarget.Rotate(rotateAxis, fEulerAdd);
        }
        if (isMove)
        {
            Vector2 delta = data.delta;
            Vector2 pos = Vector2.zero;
            if (this.isMoveZ)
            {
                float fMove = -delta.y * this.vValueScale;
                if (isReverse)
                    fMove = -fMove;
                pos.y = fMove;
                //pos.z = Mathf.Clamp(pos.z + fMove, vMoveXMin, vMoveXMax);
            }

            if (this.isMoveX)
            {
                float fMove = -delta.x * this.vValueScale;
                if (isReverse)
                    fMove = -fMove;
                pos.x = fMove;
                //pos.x = Mathf.Clamp(pos.x + fMove, vMoveZMin, vMoveZMax);
            }
            scrollDelta = new Vector3(pos.x * 0.01f, 0f, pos.y * 0.01f);
        }

        //Quaternion q = Quaternion.Euler(0f, -transTarget.eulerAngles.y, 0f);
        //pos = q * pos;
        //pos.y = 0f;

        //if (m_DraggingIcon != null)
        //    SetDraggedPosition(data);
    }
    void FixedUpdate()
    {
        if (!isMove)
            return;
        if (Mathf.Abs(Vector3.Distance(scrollDelta, Vector3.zero)) > 0.0001f)
        {
            if (Target != null)
            {
                Target.localPosition += scrollDelta;
            }
            ClampPosition();

            float elast = IsDraging ? Time.deltaTime * 12f : Time.deltaTime * Elasticity;
            scrollDelta = Vector3.Lerp(scrollDelta, Vector3.zero, elast);
        }
        
    }
    public void ClampPosition()
    {
        Vector3 pos = Target.position;
        pos.x = Mathf.Clamp(pos.x, mClampRect.min.x, mClampRect.max.x);
        pos.z = Mathf.Clamp(pos.z, mClampRect.min.y, mClampRect.max.y);
        Target.position = pos;
    }
    public void SetClamp(float x, float y, float width, float height)
    {
        this.mClampRect.Set(x, y, width, height);
    }
	private void SetDraggedPosition(PointerEventData data)
	{
        //if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
        //    m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		
        //var rt = m_DraggingIcon.GetComponent<RectTransform>();
        //Vector3 globalMousePos;
        //if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        //{
        //    rt.position = globalMousePos;
        //    rt.rotation = m_DraggingPlane.rotation;
        //}
	}

	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		var comp = go.GetComponent<T>();

		if (comp != null)
			return comp;
		
		Transform t = go.transform.parent;
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}
		return comp;
	}

    void OnDrawGizmos()
    {
        if(this.isActiveAndEnabled && this.isMove)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(mClampRect.center.ToVector3(), mClampRect.size.ToVector3());
        }
    }
}
