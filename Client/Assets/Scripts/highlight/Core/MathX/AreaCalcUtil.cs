using UnityEngine;
using System.Collections;
namespace highlight
{
    public static class AreaCalcUtil
    {
        /// <summary>
        /// 检测 target 点是否在 圆/环/弧形内
        /// </summary>
        /// <param name="start">圆/环/弧心</param>
        /// <param name="face">面向</param>
        /// <param name="target">目标坐标</param>
        /// <param name="r">最大半径</param>
        /// <param name="angle">角度 0-360</param>
        /// <param name="minR">最小半径</param>
        /// <returns></returns>
        /// 
        public static bool CalcRing3D(Vector3 _start, Vector3 _face, Vector3 _target, float r, float angle, float minR)
        {
            return CalcRing2D(new Vector2(_start.x, _start.z), new Vector2(_face.x, _face.z), new Vector2(_target.x, _target.z), r, angle, minR);
        }
        public static bool CalcRing2D(Vector2 start, Vector2 face, Vector2 target, float r, float angle, float minR)
        {
            //判断距离
            float dis = Vector2.Distance(target, start);
            if ((dis > r) || (r < minR))
                return false;

            //圆形区域
            if (angle % 360 == 0)
            {
                return true;
            }
            //扇形区域
            if (angle < 360 && angle > 0)
            {
                Vector2 toTarDir1 = target - start; //玩家到目标的方向向量
                float a = Vector2.Angle(toTarDir1, face);
                if (a < angle * 0.5f)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检测 target 点是否在 矩形内 
        /// </summary>
        /// <param name="start">矩形中心</param>
        /// <param name="face">面向</param>
        /// <param name="target">目标坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="length"><0时为射线矩形</param>
        /// <returns></returns>
        public static bool CalcRect3D(Vector3 start, Vector3 face, Vector3 target, float width, float length)
        {
            return CalcRect2D(new Vector2(start.x, start.z), new Vector2(face.x, face.z), new Vector2(target.x, target.z), width, length);
        }
        public static bool CalcRect2D(Vector2 start, Vector2 face, Vector2 target, float width, float length)
        {
            Vector2 toTarDir1 = target - start; //玩家到目标的方向向量
            float a = Vector2.Angle(toTarDir1, face);
            if (toTarDir1.normalized.y > face.normalized.y)
                a = 360f - a;
            a = a * Mathf.Deg2Rad;
            float l = toTarDir1.magnitude;
            float wh = Mathf.Sin(a) * l;
            float he = Mathf.Cos(a) * l;
            //Debug.Log(string.Format("{0},{1},Angle:{2},cos:{3}", toTarDir1, face, a, Mathf.Cos(a)));
            if (he >= 0 && (Mathf.Abs(wh) < width / 2) && (Mathf.Abs(he) < length || length <= 0f))
                return true;
            return false;
        }

        //根据角色半径r、角度angle 0-360,获得围绕center的位置点 圆
        public static Vector2 GetCirclePos(Vector2 center, float r, float angle)
        {
            return new Vector2(center.x + r * Mathf.Cos(angle), center.y + r * Mathf.Sin(angle));
        }
        /*
        /// <summary>
        /// 园，扇形
        /// </summary>
        /// <param name="parentCenter"></param>
        /// <param name="r"> 半径 </param> 
        /// <param name="angle"> 角度 0 - 360 </param>
        /// <param name="minR"> 最小半径 0 为实心园 </param>
        /// <param name="segments"> 三角形数量 </param>
        public static GameObject CreatRing(GameObject parentCenter, float r, float angle, float minR, int segments)
        {
            PrimitivesPro.GameObjects.Ring shapeObject = PrimitivesPro.GameObjects.Ring.Create(minR, r, segments, angle);
            shapeObject.transform.SetParent(parentCenter.transform);
            shapeObject.transform.localRotation = Quaternion.Euler(0f, -angle * 0.5f, 0f);
            shapeObject.transform.localPosition = Vector3.zero;
            return shapeObject.gameObject;
        }

        /// <summary>
        /// 创建一个矩形
        /// </summary>
        /// <param name="parentCenter"></param>
        /// <param name="width"> 宽 </param>
        /// <param name="length"> 长 </param>
        /// <param name="widthSegments"> 宽三角形数量 </param>
        /// <param name="lengthSegments">  </param>
        public static GameObject CreatRect(GameObject parentCenter, float width, float length, int widthSegments, int lengthSegments)
        {
            PrimitivesPro.GameObjects.PlaneObject shapeObject = PrimitivesPro.GameObjects.PlaneObject.Create( width, length, widthSegments, lengthSegments);
            MeshRenderer mRender = shapeObject.GetComponent<MeshRenderer>();
            //Shader shaderNew = Shader.Find(sShaderName);
            //mRender.
            shapeObject.transform.SetParent(parentCenter.transform);
            shapeObject.transform.localRotation = Quaternion.identity;
            shapeObject.transform.localPosition = new Vector3(0f, 0f, length*0.5f);
            return shapeObject.gameObject;
        }

        /// <summary>
        /// 更新矩形区域对象，这个用在通过外面创建好预设然后在这里刷新属性
        /// </summary>
        /// <param name="goRect"></param>
        /// <param name="parentCenter"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="widthSegments"></param>
        /// <param name="lengthSegments"></param>
        public static void UpdateRect(GameObject goRect, GameObject parentCenter, float width, float length, int widthSegments, int lengthSegments)
        {
            PlaneObject pObject = goRect.GetComponentInChildren<PlaneObject>(true);
            pObject.length = length;
            pObject.width = width;
            pObject.lengthSegments = lengthSegments;
            pObject.widthSegments = widthSegments;
            pObject.GenerateGeometry();
            goRect.transform.SetParent(parentCenter.transform);
            goRect.transform.localRotation = Quaternion.identity;
            goRect.transform.localPosition = new Vector3(0f, 0f, length * 0.5f);
        }

        /// <summary>
        /// 更新圆形区域
        /// </summary>
        /// <param name="goRing"></param>
        /// <param name="parentCenter"></param>
        /// <param name="r"></param>
        /// <param name="angle"></param>
        /// <param name="minR"></param>
        /// <param name="segments"></param>
        public static void UpdateRing(GameObject goRing, GameObject parentCenter, float r, float angle, float minR, int segments)
        {
            Ring pObject = goRing.GetComponentInChildren<Ring>(true);
            pObject.radius0 = minR;
            pObject.radius1 = r;
            pObject.angle = angle;
            pObject.segments = segments;
            pObject.GenerateGeometry();
            goRing.transform.SetParent(parentCenter.transform);
            goRing.transform.localRotation = Quaternion.Euler(0f, -angle * 0.5f, 0f);
            goRing.transform.localPosition = Vector3.zero;
            //PlaneObject pObject = goRect.GetComponent<PlaneObject>();
        }
        public static void UpdateRing(GameObject goRing, float min,float max)
        {
            Ring pObject = goRing.GetComponentInChildren<Ring>(true);
            pObject.radius0 = min;
            pObject.radius1 = max;
            pObject.angle = 360;
            pObject.segments = 4;
            pObject.GenerateGeometry();
        }
        */
        public static bool IsPointInPolygon(Vector3 p, Vector3[] poly)
        {
            if (poly == null || poly.Length < 3)
                return false;
            bool c = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].z > p.z) != (poly[j].z > p.z)) && (p.x < (poly[j].x - poly[i].x) * (p.z - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x))
                    c = !c;
            }
            return c;
        }
        public static bool IsPointInPolygon(Vector2 p, Vector2[] poly)
        {
            if (poly == null || poly.Length < 3)
                return false;
            // Algorithm from http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            // translated into C#
            bool c = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ( ((poly[i].y > p.y) != (poly[j].y > p.y)) && (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x) )
                    c = !c;
            }
            return c;
        }
    }
}