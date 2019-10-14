using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight.tl
{
    [Action("行为/设置旋转", typeof(SetRotationAction))]
    public class SetRotationAction : TargetAction
    {
        public static float RotateSpeed = 180f;
        [Desc("结束挂点")]
        public IVector3 end;
        public override TriggerStatus OnTrigger()
        {
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            // AttrBoolData data = this.targetData
            Vector3 forward = GetForward(this.role.position, this.end.vec3, (Vector3)this.role.forward);
            this.role.SetForward(forward, false);
        }

        public static Vector3 GetForward(Vector3 start, Vector3 end, Vector3 sForward,float speed = 1f)
        {
            Quaternion to = Quaternion.identity;
            Vector3 dir = end - start;
            if(dir != Vector3.zero)
                to = Quaternion.LookRotation(dir);
            Quaternion cur = Quaternion.LookRotation(sForward);
            Quaternion q = Quaternion.RotateTowards(cur, to, App.logicDeltaTime * RotateSpeed * speed);
            return q * Vector3.forward;
        }
        public static Vector3 Slerp(Vector3 sForward, Vector3 toForward,  float progress)
        {
            Quaternion to = Quaternion.identity;
            if (toForward != Vector3.zero)
                to = Quaternion.LookRotation(toForward);
            Quaternion cur = Quaternion.LookRotation(sForward);
            Quaternion q = Quaternion.Slerp(cur, to, progress);
            return q * Vector3.forward;
        }
    }
}