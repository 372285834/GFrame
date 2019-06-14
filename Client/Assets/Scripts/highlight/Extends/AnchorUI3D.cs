using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AnchorUI3D : MonoBehaviour
{
    public Transform target;
    public Camera Camera3D;
    public Camera UICamera;
    public LayerMask tLayer;
    public static Ray ray;
    public static float length = 1000f;
    public static AnchorUI3D Get(GameObject go)
    {
        return go.AddComp<AnchorUI3D>();
    }
    private void Update()
    {
        if(UICamera != null && Camera3D != null && target != null)
        {
            InitBuyWorldPos();
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * length, Color.red);
#endif
        }
    }
    public void InitBuyWorldPos()
    {
        RaycastHit hitInfo = SetPosByWorldPos(this.transform.position, UICamera, Camera3D, tLayer.value);
        if(hitInfo.collider != null)
        {
            target.position = hitInfo.point;
        }
    }
    //public void InitBuyScreenPos(Vector3 sPos)
    //{
    //    SetPosByScreenPos(this.transform, sPos, Camera3D, tLayer.value);
    //}
    public static Ray GetRay(Vector3 wPos1,Camera uiCamera, Camera camera2)
    {
        Vector3 spos = RectTransformUtility.WorldToScreenPoint(uiCamera, wPos1); //camera1.WorldToViewportPoint(wPos1);
        ray = camera2.ScreenPointToRay(spos);
        return ray;
    }
    public static RaycastHit SetPosByWorldPos(Vector3 wPos1, Camera uiCamera, Camera camera2,int layer)
    {
        Ray ray = GetRay(wPos1, uiCamera, camera2);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray.origin,ray.direction * length, out hitInfo, length, layer))
        {
        }
        return hitInfo;
    }
    public static RaycastHit SetPosByScreenPos(Vector3 sPos, Camera camera, int layer)
    {
        Ray ray = camera.ScreenPointToRay(sPos);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray.origin, ray.direction * length, out hitInfo, length, layer))
        {
        }
        return hitInfo;
    }
}
