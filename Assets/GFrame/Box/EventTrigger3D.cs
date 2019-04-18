using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger3D : MonoBehaviour
{
    public int id;
    public LayerMask layerMask;
    public GameObject target;
    public Collider curCollider;
   // [XLua.LuaCallCSharp][XLua.CSharpCallLua]
    public delegate void onTriggerEvent(EventTrigger3D trigger, int id, GameObject go);
    public onTriggerEvent onTriggerEnter = null;
    public onTriggerEvent onTriggerStay = null;
    public onTriggerEvent onTriggerExit = null;
    private void Awake()
    {
        if (target == null)
            target = this.gameObject;
    }
    void OnTriggerEnter(Collider co)
    {
        int layer = co.gameObject.layer;
        //Debug.Log(this.name + "-------" + co.gameObject.name);
        if (layerMask.Contains(layer))
        {
            if (onTriggerEnter != null)
            {
                curCollider = co;
                int id = GetMapId(co.gameObject);
                onTriggerEnter(this, id, co.gameObject);
            }
                
        }
    }
    void OnTriggerStay(Collider co)
    {
        int layer = co.gameObject.layer;
        //Debug.Log(this.name + "-------" + co.gameObject.name);
        if (layerMask.Contains(layer))
        {
            if (onTriggerStay != null)
            {
                int id = GetMapId(co.gameObject);
                onTriggerStay(this, id, co.gameObject);
            }
        }
    }
    void OnTriggerExit(Collider co)
    {
        int layer = co.gameObject.layer;
        //Debug.Log(this.name + "-------" + co.gameObject.name);
        if (layerMask.Contains(layer))
        {
            if (onTriggerExit != null)
            {
                int id = GetMapId(co.gameObject);
                onTriggerExit(this, id, co.gameObject);
                curCollider = null;
            }
        }
    }
    private void OnDisable()
    {
        if (curCollider != null)
            OnTriggerExit(curCollider);
    }
    public static int GetMapId(GameObject go)
    {
        MapItemMono mono = go.GetComponentInParent<MapItemMono>();
        if (mono != null)
            return mono.mapId;
        return -1;
    }
    public static EventTrigger3D AddTriggerNames(GameObject go, params string[] layerNames)
    {
        return AddTrigger(go, LayerMask.GetMask(layerNames));
    }
    public static EventTrigger3D AddTrigger(GameObject go, int layerMask)
    {
        EventTrigger3D trigger = go.AddComp<EventTrigger3D>();
        trigger.layerMask = layerMask;
        return trigger;
    }

    private static int _HideLayer = -1;
    public static int HideLayer
    {
        get
        {
            if (_HideLayer == -1)
                _HideLayer = LayerMask.NameToLayer("Hide");
            return _HideLayer;
        }
    }
    private static int _DissoveLayer = -1;
    public static int DissoveLayer
    {
        get
        {
            if (_DissoveLayer == -1)
                _DissoveLayer = LayerMask.NameToLayer("Dissove");
            return _DissoveLayer;
        }
    }
    private static int _WaterLayer = -1;
    public static int WaterLayer
    {
        get
        {
            if (_WaterLayer == -1)
                _WaterLayer = LayerMask.NameToLayer("Water");
            return _WaterLayer;
        }
    }
    private static int _HideCameraLayer = -1;
    public static int HideCameraLayer
    {
        get
        {
            if (_HideCameraLayer == -1)
                _HideCameraLayer = LayerMask.NameToLayer("HideCamera");
            return _HideCameraLayer;
        }
    }
}
