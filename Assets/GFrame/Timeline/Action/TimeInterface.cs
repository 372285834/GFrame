using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public interface ITransform
    {
        Transform transform { get; }
    }
    public interface ITarget
    {
        Transform transform { get; }
    }
    public interface IParent
    {
        Transform transform { get; }
    }
    public interface IPosition2
    {
        Vector3 start { get; }
        Vector3 end { get; }
    }

    public interface IPosition
    {
        Vector3 getPosition { get; }
    }
    public interface IScale
    {
        Vector3 getScale { get; }
    }
    public interface IRotation
    {
        Vector3 getRotation { get; }
    }
    public interface IEvaluate
    {
        Vector3 Evaluate(Vector3 start, Vector3 end, float time);
    }
}