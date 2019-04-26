using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
namespace highlight
{
    public static class App
    {
        public static Timer Timer = new Timer();
        public static int deltaFrame { private set; get; }
        static bool isInit = false;
        public static Observer obsUpdate = new Observer();
        public static void Init()
        {
            if (isInit)
                return;
            isInit = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Update;
            UnityEditor.EditorApplication.update += Update;
#endif
        }
        public static Vector3 downPos;
        public static float moveDis
        {
            get
            {
                return Vector3.Distance(downPos, Input.mousePosition);
            }
        }
        public static void Update()
        {
            deltaFrame += 1;
            if (IsDown())
                downPos = Input.mousePosition;
            UpdateShaderTime();
          //  LabelRoll.UpdateMaterila();
            Timer.update();
            obsUpdate.Change();
           // Text3DShadow.UpdateCommandBuffer();
        }
        public static void LateUpdate()
        {

        }
        public static float ShaderTime = 0f;
        public static void UpdateShaderTime()
        {
            if (ShaderTime > 1800f)
                ShaderTime = 0f;
            ShaderTime += Time.deltaTime * 0.1f;
            float t = ShaderTime % 314.15926f;
            //Shader.SetGlobalFloat(Frame.Const._MShaderTimeS, t);
            //Shader.SetGlobalFloat(Frame.Const._MShaderTime, ShaderTime % 300);
            //if (ThreeLockCamera.Main != null)
            //    Shader.SetGlobalVector(Frame.Const._MWorldCenterPos, ThreeLockCamera.Main.transform.position);
        }

        public static void Destroy()
        {
            Timer.destroy();
        }
        public static void MousePickMobile()
        {
            MousePick();
        }
        //点击屏幕返回选中的对象
        public static RaycastHit[] MousePick()
        {
            if (Camera.main == null)
                return null;
            RaycastHit[] hits = null;
            bool isDown = false;
            bool isUp = false;
#if UNITY_EDITOR
            isDown = Input.GetMouseButtonDown(0);
            isUp = Input.GetMouseButtonUp(0);
            if (isDown || isUp)
            {
                if (IsIgnoreRay() && isDown)
                {
                    return null;
                }
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hits = Physics.RaycastAll(ray, 1000f);
                {
                    //Debug.Log(hit.point.ToString() + "," + hit.transform.name);
                }
            }
#else        
            if (Input.touchCount == 1)
            {
			    Touch touch = Input.GetTouch(0);
			    isDown = touch.phase == TouchPhase.Began;
			    isUp = touch.phase == TouchPhase.Ended;
                if (IsIgnoreRay() && isDown)
                {
                    return null;
                }
			    if (isDown || isUp)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    hits = Physics.RaycastAll(ray, 1000f);
                    {
                        //Debuger.Log(hit.transform.name);
                    }
                }
            }
		    else
		    {
			    //isUp = true;
		    }
#endif
            //Button3DEvent(hits, isDown, isUp);
            return hits;
        }
        public static bool IsDown()
        {
            bool isDown = false;
#if UNITY_EDITOR
            isDown = Input.GetMouseButtonDown(0);
#else    
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            isDown = touch.phase == TouchPhase.Began;
        }
#endif
            return isDown;
        }
        public static bool IsUp()
        {
            bool isUp = false;
#if UNITY_EDITOR
            isUp = Input.GetMouseButtonUp(0);
#else    
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            isUp = touch.phase == TouchPhase.Ended;
        }
#endif
            return isUp;
        }
        public static bool IsIgnoreRay()
        {
            //#if UNITY_EDITOR
            //if (!EventSystem.current.IsPointerOverGameObject())
            //	return false;
            //#else    
            GameObject cur = EventSystem.current.currentSelectedGameObject;
            if (cur == null)
            {
                PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                eventData.pressPosition = Input.mousePosition;
                eventData.position = Input.mousePosition;
                List<RaycastResult> list = new List<RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, list);
                if (list.Count == 0)
                    return false;
                //else
                //{
                //    for (int i = 0; i < list.Count; i++)
                //    {
                //        if (list[i].gameObject != null && list[i].gameObject.GetComponent<RayIgnore>() == null)
                //            return true;
                //    }
                //    return false;
                //}
            }
            //else if (cur.GetComponent<RayIgnore>() != null)
            //{
            //    return false;
            //}
            return true;
        }

       
    }
}