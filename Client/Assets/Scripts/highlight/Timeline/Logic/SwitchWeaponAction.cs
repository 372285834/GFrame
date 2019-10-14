using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight.tl
{
    [Action("自定义/行为/切换武器", typeof(SwitchWeaponAction))]
    public class SwitchWeaponAction : TimeAction
    {
        [Desc("武器id_int")]
        public CountData data;
        public int curId;
        public override TriggerStatus OnTrigger()
        {
            //if (data == null)
            //    return TriggerStatus.Failure;
            //if (this.owner.items == null)
            //    return TriggerStatus.Failure;
            //curId = this.owner.items.weaponId;
            //this.owner.items.SwitchWeapon(data.value);
            return TriggerStatus.Success;
        }
        public override void OnFinish()
        {
            //this.owner.items.SwitchWeapon(curId);
            base.OnFinish();
        }
    }
}