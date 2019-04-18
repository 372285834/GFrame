using UnityEngine;
using System.Collections;

/// <summary>
/// 必须继承PropertyAttribute
/// </summary>
public class Vector2IntAtt : PropertyAttribute
{

    //绘制需要的数据
    public int x;
    public int y;
    public string label;

    public Vector2IntAtt(int x, int y, string label = "")
    {
        this.x = x;
        this.y = y;
        this.label = label;
    }
}