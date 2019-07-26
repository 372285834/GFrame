using UnityEngine;
using System.Collections;
namespace highlight
{
    public static class VectorTools
    {
        public static float RandomValue(this Vector2 clamp)
        {
            return Random.Range(clamp.x, clamp.y);
        }
        public static Vector3 GetPos(int x, int y, float offx, float offy, Vector3 pos, Vector2 cell)
        {
            return new Vector3((x + 0.5f + offx) * cell.x + pos.x, 0f, (y + 0.5f + offy) * cell.y + pos.z);
        }
        public static Vector2Int GetXY(Vector3 pos, Vector3 start, Vector2 cell)
        {
            pos = pos - start;
            float row = (pos.x / cell.x);
            float column = (pos.z / cell.y);
            Vector2Int vec = new Vector2Int((int)row, (int)column);
            return vec;
        }
        public static Vector2 ToV2(this Vector3 pos, float off=0)
        {
            return new Vector2(pos.x + off, pos.z + off);
        }
        public static Vector2 ToVector2(this Vector3 pos, float off)
        {
            return new Vector2(pos.x + off, pos.z + off);
        }
        public static Vector2 ToVector2(this Vector3 pos)
        {
            return new Vector2(pos.x, pos.z);
        }
        public static Vector3 ToVector3(this Vector2 pos, float h = 0)
        {
            return new Vector3(pos.x, h, pos.y);
        }
        public static Vector3 LerpSpeed(Vector3 start,Vector3 end,float length)
        {
            float dis = Vector3.Distance(start, end);
            if (length > dis)
                length = dis;
            Vector3 dir = (end - start).normalized;
            Vector3 to = start + dir * length;
            return to;
        }
        public static Rect AddRectSize(this Rect rect, Vector2 size)
        {
            return new Rect(rect.min - size, rect.size + size * 2f);
        }
        public static Rect AddRectSize(this Rect rect, float size)
        {
            return AddRectSize(rect, new Vector2(size, size));
        }
        public static Vector3 ClampPosition(this Rect rect, Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, rect.min.x, rect.max.x);
            pos.z = Mathf.Clamp(pos.z, rect.min.y, rect.max.y);
            return pos;
        }
        public static uint calculatePageIDByPos(Vector3 pos, Vector2 cell)
        {
            float row = (pos.x / cell.x);
            float column = (pos.z / cell.y);
            ushort x = (ushort)row;
            ushort z = (ushort)column;
            return calculatePageID(x, z);
        }
        public static uint calculatePageID(int x, int z)
        {
            return calculatePageID((ushort)x, (ushort)z);
        }
        public static uint calculatePageID(ushort x, ushort z)
        {
            uint x16 = (uint)x;
            uint z16 = (uint)z;
            //ulong key = x * 1000000 + z;
            uint key = (x16 << 16) | z16;
            return key;
        }

        public static void calculateCell(uint inPageID, out ushort x, out ushort y)
        {
            x = (ushort)((inPageID >> 16) & 0xFFFF);
            y = (ushort)(inPageID & 0xFFFF);
        }

        public static Rect CreatRect(Vector3[] points, float off = 0f)
        {
            Rect rect = new Rect(0f, 0f, 1f, 1f);
            if (points.Length == 0)
                return rect;
            if (points.Length > 1)
            {
                Vector3 max = points[0];
                Vector3 min = points[0];
                for (int i = 1; i < points.Length; i++)
                {
                    max = Vector3.Max(max, points[i]);
                    min = Vector3.Min(min, points[i]);
                }
                rect.min = min.ToVector2(-off);
                rect.max = max.ToVector2(off);
                //Vector3 size = max - min;
                //Vector3 center = min + size * 0.5f;
                //Vector2 size2 = new Vector2(size.x + off, size.z + off);
                //rect = new Rect(new Vector2(center.x, center.z), size2);
            }
            else
            {
                rect.center = new Vector2(points[0].x, points[0].z);
                rect.size = Vector2.one;
            }
            return rect;
        }


        public static void DrawLine(Color co, Vector3 start, Vector2 cell, int x, int y)
        {
            if (co.a <= 0f)
                return;
            float endx = x * cell.x + start.x;
            for (int i = 0; i <= x; i++)
            {
                float value = i * cell.y + start.z;
                Debug.DrawLine(new Vector3(start.x, start.y, value), new Vector3(endx, start.y, value), co);
            }
            float endz = y * cell.y + start.z;
            for (int j = 0; j <= y; j++)
            {
                float value = j * cell.x + start.x;
                Debug.DrawLine(new Vector3(value, start.y, start.z), new Vector3(value, start.y, endz), co);
            }
        }


        /// <summary>
        /// 点到直线距离
        /// </summary>
        /// <param name="point">点坐标</param>
        /// <param name="linePoint1">直线上一个点的坐标</param>
        /// <param name="linePoint2">直线上另一个点的坐标</param>
        /// <returns></returns>
        public static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
        {
            Vector3 vec1 = point - linePoint1;
            Vector3 vec2 = linePoint2 - linePoint1;
            Vector3 vecProj = Vector3.Project(vec1, vec2);
            float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
            return dis;
        }

        /// <summary>
        /// 点到平面的距离 自行推演函数
        /// </summary>
        /// <param name="point"></param>
        /// <param name="surfacePoint1"></param>
        /// <param name="surfacePoint2"></param>
        /// <param name="surfacePoint3"></param>
        /// <returns></returns>
        public static float DisPoint2Surface(Vector3 point, Vector3 surfacePoint1, Vector3 surfacePoint2, Vector3 surfacePoint3)
        {
            //空间直线一般式方程 Ax + By + Cz + D = 0;
            //假定 A = 1 ，推演B C D用A来表示，约去A，可得方程
            float BNumerator = (surfacePoint1.x - surfacePoint2.x) * (surfacePoint2.z - surfacePoint3.z) - (surfacePoint2.x - surfacePoint3.x) * (surfacePoint1.z - surfacePoint2.z);
            float BDenominator = (surfacePoint2.y - surfacePoint3.y) * (surfacePoint1.z - surfacePoint2.z) - (surfacePoint1.y - surfacePoint2.y) * (surfacePoint2.z - surfacePoint3.z);
            float B = BNumerator / BDenominator;
            float C = (B * (surfacePoint1.y - surfacePoint2.y) + (surfacePoint1.x - surfacePoint2.x)) / (surfacePoint2.z - surfacePoint1.z);
            float D = -surfacePoint1.x - B * surfacePoint1.y - C * surfacePoint1.z;

            return DisPoint2Surface(point, 1f, B, C, D);
        }

        public static float DisPoint2Surface(Vector3 point, float FactorA, float FactorB, float FactorC, float FactorD)
        {
            //点到平面的距离公式 d = |Ax + By + Cz + D|/sqrt(A2 + B2 + C2);
            float numerator = Mathf.Abs(FactorA * point.x + FactorB * point.y + FactorC * point.z + FactorD);
            float denominator = Mathf.Sqrt(Mathf.Pow(FactorA, 2) + Mathf.Pow(FactorB, 2) + Mathf.Pow(FactorC, 2));
            float dis = numerator / denominator;
            return dis;
        }

        /// <summary>
        /// 点到平面距离 调用U3D Plane类处理
        /// </summary>
        /// <param name="point"></param>
        /// <param name="surfacePoint1"></param>
        /// <param name="surfacePoint2"></param>
        /// <param name="surfacePoint3"></param>
        /// <returns></returns>
        public static float DisPoint2Surface2(Vector3 point, Vector3 surfacePoint1, Vector3 surfacePoint2, Vector3 surfacePoint3)
        {
            Plane plane = new Plane(surfacePoint1, surfacePoint2, surfacePoint3);

            return DisPoint2Surface2(point, plane);
        }

        public static float DisPoint2Surface2(Vector3 point, Plane plane)
        {
            return plane.GetDistanceToPoint(point);
        }

        /// <summary>
        /// 平面夹角
        /// </summary>
        /// <param name="surface1Point1"></param>
        /// <param name="surface1Point2"></param>
        /// <param name="surface1Point3"></param>
        /// <param name="surface2Point1"></param>
        /// <param name="surface2Point2"></param>
        /// <param name="surface2Point3"></param>
        /// <returns></returns>
        public static float SurfaceAngle(Vector3 surface1Point1, Vector3 surface1Point2, Vector3 surface1Point3, Vector3 surface2Point1, Vector3 surface2Point2, Vector3 surface2Point3)
        {
            Plane plane1 = new Plane(surface1Point1, surface1Point1, surface1Point1);
            Plane plane2 = new Plane(surface2Point1, surface2Point1, surface2Point1);
            return SurfaceAngle(plane1, plane2);
        }

        public static float SurfaceAngle(Plane plane1, Plane plane2)
        {
            return Vector3.Angle(plane1.normal, plane2.normal);
        }

        public static float Vector3Angle(Vector3 from, Vector3 to)
        {
            Vector3 pos = to - from;
            float angle = Vector3.Angle(from, to);
            if (pos.x < 0)
                angle = -angle;
            return angle;
        }

        public static void Bresenhamline(int x0, int y0, int x1, int y1, int color)
        {
            int x, y, dx, dy;
            float k, e;
            dx = x1 - x0; dy = y1 - y0; k = dy / dx;
            e = -0.5f; x = x0; y = y0;
            for (int i = 0; i <= dx; i++)
            {    //drawpixel (x, y, color);
                x = x + 1; e = e + k;
                if (e >= 0)
                {
                    y++; e = e - 1;
                }
            }
        }
        //或者将e扩大2dx倍；
        public static void Bresenhamline2dx(int x0, int y0, int x1, int y1, int color)
        {
            int x, y, dx, dy;
            float k, e;
            dx = x1 - x0; dy = y1 - y0; k = dy / dx;
            e = -dx; x = x0; y = y0;
            for (int i = 0; i <= dx; i++)
            {// drawpixel (x, y, color);
                x = x + 1; e = e + 2 * dy;
                if (e >= 0)
                { y++; e = e - 2 * dx; }
            }
        }
    }
}