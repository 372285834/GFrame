using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("行为/播放特效", typeof(PlayEffectAction))]
    public class PlayEffectAction : TimeAction
    {
        [Desc("挂点")]
        public LocatorData data;
        EffectObject mobj;
        public override TriggerStatus OnTrigger()
        {
            if (this.timeObject.resData == null)
                return TriggerStatus.Failure;
          //  Debug.Log("EffectManager Create:");
            EffectManager.Instance.Create(this.timeObject.resData.mStyle.res, CreatCallBack);
            return TriggerStatus.Success;
        }
        //public override void OnUpdate()
        //{

        //}
        public void CreatCallBack(EffectObject effectObj)
        {
            mobj = effectObj;
            mobj.transform.position = data.vec3;
        }
        public override void OnFinish()
        {
            if (mobj != null)
                EffectManager.Instance.Destroy(mobj);
            mobj = null;
            base.OnFinish();
        }
    }
}