
using System.Collections;
using System.Collections.Generic;
namespace highlight.tl
{
    [Action("全局/修改全局属性", typeof(GlobalAction))]
    public class GlobalAction : TimeAction
    {
        [Desc("全局属性")]
        public GlobalData data;
        public override TriggerStatus OnTrigger()
        {
            if(data is GlobalIntData)
                this.root.SetGlobalValue(data.key, (data as GlobalIntData).value);
            else if(data is GlobalStringData)
                this.root.SetGlobalValue(data.key, (data as GlobalStringData).value);
            return TriggerStatus.Success;
        }
    }
}