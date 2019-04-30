﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierUtils
{
    /// <summary>
    /// 线性贝赛尔曲线
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P1"></param>
    /// <param name="t"> 0.0 >= t <= 1.0 </param>
    /// <returns></returns>
    public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t);
        B = t1 * P0 + P1 * t;
        //B.y = t1*P0.y + P1.y*t;
        //B.z = t1*P0.z + P1.z*t;
        return B;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P1"></param>
    /// <param name="P2"></param>
    /// <param name="t">0.0 >= t <= 1.0 </param>
    /// <returns></returns>
    public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t) * (1 - t);
        float t2 = t * (1 - t);
        float t3 = t * t;
        B = P0 * t1 + 2 * t2 * P1 + t3 * P2;
        //B.y = P0.y*t1 + 2*t2*P1.y + t3*P2.y;
        //B.z = P0.z*t1 + 2*t2*P1.z + t3*P2.z;
        return B;
    }
    public static Vector2 BezierCurve2D(Vector2 P0, Vector2 P1, Vector2 P2, float t)
    {
        Vector2 B = Vector2.zero;
        float t1 = (1 - t) * (1 - t);
        float t2 = t * (1 - t);
        float t3 = t * t;
        B = P0 * t1 + 2 * t2 * P1 + t3 * P2;
        return B;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P1"></param>
    /// <param name="P2"></param>
    /// <param name="P3"></param>
    /// <param name="t">0.0 >= t <= 1.0 </param>
    /// <returns></returns>
    public static Vector3 BezierCurve(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
    {
        Vector3 B = Vector3.zero;
        float t1 = (1 - t) * (1 - t) * (1 - t);
        float t2 = (1 - t) * (1 - t) * t;
        float t3 = t * t * (1 - t);
        float t4 = t * t * t;
        B = P0 * t1 + 3 * t2 * P1 + 3 * t3 * P2 + P3 * t4;
        //B.y = P0.y*t1 + 3*t2*P1.y + 3*t3*P2.y + P3.y*t4;
        //B.z = P0.z*t1 + 3*t2*P1.z + 3*t3*P2.z + P3.z*t4;
        return B;
    }
    public static Vector2 BezierCurve2D(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3, float t)
    {
        Vector2 B = Vector2.zero;
        float t1 = (1 - t) * (1 - t) * (1 - t);
        float t2 = (1 - t) * (1 - t) * t;
        float t3 = t * t * (1 - t);
        float t4 = t * t * t;
        B = P0 * t1 + 3 * t2 * P1 + 3 * t3 * P2 + P3 * t4;
        return B;
    }
    public static Vector3[] PathControlPointGenerator(Vector3[] path)
    {
        Vector3[] suppliedPath;
        Vector3[] vector3s;

        //create and store path points:
        suppliedPath = path;

        //populate calculate path;
        int offset = 2;
        vector3s = new Vector3[suppliedPath.Length + offset];
        Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);

        //populate start and end control points:
        //vector3s[0] = vector3s[1] - vector3s[2];
        vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
        vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);

        //is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
        if (vector3s[1] == vector3s[vector3s.Length - 2])
        {
            Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
            Array.Copy(vector3s, tmpLoopSpline, vector3s.Length);
            tmpLoopSpline[0] = tmpLoopSpline[tmpLoopSpline.Length - 3];
            tmpLoopSpline[tmpLoopSpline.Length - 1] = tmpLoopSpline[2];
            vector3s = new Vector3[tmpLoopSpline.Length];
            Array.Copy(tmpLoopSpline, vector3s, tmpLoopSpline.Length);
        }
        return (vector3s);
    }

    public static Vector3 Interp(Vector3[] pts, float t)
    {
        int numSections = pts.Length - 3;
        int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
        float u = t * (float)numSections - (float)currPt;

        Vector3 a = pts[currPt];
        Vector3 b = pts[currPt + 1];
        Vector3 c = pts[currPt + 2];
        Vector3 d = pts[currPt + 3];

        return .5f * (
            (-a + 3f * b - 3f * c + d) * (u * u * u)
            + (2f * a - 5f * b + 4f * c - d) * (u * u)
            + (-a + c) * u
            + 2f * b
        );
    }


    /// <summary>
    /// 获取存储贝塞尔曲线点的数组
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>存储贝塞尔曲线点的数组
    public static Vector3[] GetBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = BezierCurve(startPoint, controlPoint, endPoint, t);
            path[i - 1] = pixel;
            //Debug.Log(path[i - 1]);
        }
        return path;

    }
}