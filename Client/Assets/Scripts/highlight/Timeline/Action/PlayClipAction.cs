using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/播放动画", typeof(PlayClipAction))]
    public class PlayClipAction : TargetAction
    {
        [Desc("动画数据")]
        public AnimatorData data;
        public CDData cd = CDData.Min;
        public float clipLength = 0;
        float _speed;
        public float speed
        {
            get
            {
                return _speed;
            }
            set
            {
                if (_speed == value)
                    return;
                _speed = value;
                cd.length = VInt.Round(clipLength / speed);
            }
        }
        public override TriggerStatus OnTrigger()
        {
            if (data == null)
                return TriggerStatus.Failure;
            AnimatorStyle style = data.mStyle;
            if (this.status == TriggerStatus.InActive)
            {
                cd = new CDData(1);
                clipLength = this.role.GetClipLength(style.clip);
                speed = style.speed;
                cd.length = VInt.Round(clipLength / speed);
                role.PlayClip(style.clip, style.loop, speed, cd.length);
                return TriggerStatus.Running;
            }

            if (style.loop)
            {
                speed = style.speed * role.ClipSpeed;
                //role.SetClipSpeed(speed);
                role.PlayClip(style.clip, style.loop, speed, cd.length);
                //if (cd.IsComplete)
                //{
                //    cd.Reset();
                //    cd.length = VInt.Round(PlayClip());
                //}
            }
            else
            {
                if (cd.IsComplete)
                {
                    return TriggerStatus.Success;
                }
            }
            return TriggerStatus.Running;
        }

    }
}