using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    public interface ITimeInterface { }
    public interface IVector3 : ITimeInterface
    {
        Vector3 vec3 { get; set; }
    }
    public interface ITransform : ITimeInterface
    {
        Transform transform { get; set; }
    }
    public interface IEvaluateV3 : ITimeInterface
    {
        Vector3 Evaluate(Vector3 start, Vector3 end, float time);
    }
    public interface ITimelineHandler : ITimeInterface
    {
        Timeline timeline { get; set; }
        TimelineStyle timelineStyle { get; set; }
    }

    public interface ICondition : ITimeInterface
    {
        bool GetBool();
    }
    public interface IStateValue: ITimeInterface
    {
        int GetCurValue();
        int value { get; }
        string key { get; }
        void Switch();
        void Regist(AcHandler ac);
    }
}