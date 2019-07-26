using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace highlight.tl
{
    [Action("行为/寻敌_范围_持续", typeof(FindTarget_Rang_Action))]
    public class FindTarget_Rang_Action : TimeAction
    {
        [Desc("挂点")]
        public IVector3 data;
        [Desc("数量")]
        public CountData num;
        [Desc("范围")]
        public CountData rang;
        public static int optimization = 2;
        public override TriggerStatus OnTrigger()
        {
            return TriggerStatus.Success;
        }
        public override void OnUpdate()
        {
            //if ((App.frame + this.owner.onlyId) % optimization != 0)
            //    return;
            Vector3 pos = this.owner.position;
            if (data != null)
                pos = data.vec3;
            int max = num == null ? int.MaxValue : num.value;
            RoleManager.FindInOut(this.target, RoleType.Monster, pos, rang.value * 0.001f, max);
           // if (this.root.target.inObjects.Count > 0)
          //      Debug.Log("inObjects:" + this.root.target.inObjects.Count);
           // cur-= this.root.target.inObjects.Count;
        }

        public override void OnStop()
        {
            base.OnStop();
        }

#if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            GUILayout.Label("cur:" + this.target.mObjects.Count);
        }
        public override void OnDrawGizmos()
        {
            Vector3 pos = this.owner.position;
            if (data != null)
                pos = data.vec3;
            //Gizmos.DrawWireSphere(pos, rang.value * 0.001f);
            Handles.DrawWireDisc(pos, Vector3.up, rang.value * 0.001f);
        }
#endif
    }
}