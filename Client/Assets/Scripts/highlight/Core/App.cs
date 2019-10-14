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
        public static int deltaTime_Mill;
        public static int frame;
        public static int time;//毫秒
        public static float logicDeltaTime = 0.066f;
        public static int Acceleration = 1;
        public static int RenderFrameRate = 60;
        public static float render_time = 0;
        public static float nextLogicTime = 0;
        public static bool stand_alone = true;
        static bool isInit = false;
        public static Observer obs_update = new Observer();
        public static Observer obs_second = new Observer();

        public static void Init()
        {
            //if (isInit)
            //    return;
            //isInit = true;
//#if UNITY_EDITOR
//            if(!Application.isPlaying)
//            {
//                UnityEditor.EditorApplication.update -= Update;
//                UnityEditor.EditorApplication.update += Update;
//            }
//#endif
        }
        public static Vector3 downPos;
        public static float moveDis
        {
            get
            {
                return Vector3.Distance(downPos, Input.mousePosition);
            }
        }
        public static void UpdateLogic(int delta)
        {
            frame += delta;
            deltaFrame = delta;
            RoleManager.Update(delta);
        }
        public static void UpdateRender(float interpolation)
        {
            RoleManager.UpdateRender(interpolation);
        }
        static float curTime;
        public static float interpolationTime;
        public static bool excFrame = false;
        public static void Update()
        {
            if (IsDown())
                downPos = Input.mousePosition;
            UpdateShaderTime();
            float delta = Time.deltaTime;
            render_time += delta;
            excFrame = false;
            if (render_time > nextLogicTime)
            {
               // if(Events.Length > 0)
                {
                    excFrame = true;
                    Events.Update();
                    nextLogicTime += logicDeltaTime;
                    int detalFrame = Mathf.CeilToInt(RenderFrameRate * logicDeltaTime);
                    deltaTime_Mill = Mathf.RoundToInt(logicDeltaTime * 1000);
                    time += deltaTime_Mill;
                    UpdateLogic(detalFrame);
                }
            }
           // Debug.Log(interpolationTime + "," + excFrame);
            //  LabelRoll.UpdateMaterila();
            Timer.update();
            obs_update.Change();
            curTime += Time.deltaTime;
            if(curTime >= 1f)
            {
                curTime = 0f;
                obs_second.Change();
            }
            // Text3DShadow.UpdateCommandBuffer();
        }
        public static KeyMoveEvent keyMove = new KeyMoveEvent();
        public static void LateUpdate()
        {
            if (stand_alone)
            {
                keyMove.Update();
                if (Events.Length == 0)
                    Events.Enqueue();
            }
            if (interpolationTime <= 1f || excFrame)
            {
                interpolationTime = (render_time + logicDeltaTime - nextLogicTime) / logicDeltaTime;
                if (interpolationTime < 0f || interpolationTime > 1f)
                    interpolationTime = 1f;
                UpdateRender(interpolationTime);
            }
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