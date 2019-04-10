using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.timeline
{
    public interface ITimeInterface { }
    public interface IPosition : ITimeInterface
    {
        Vector3 pos { get; set; }
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
}