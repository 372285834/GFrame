using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
    public class TargetAction : TimeAction
    {
        [Desc("目标")]
        public TargetData targetData;
        public Role targetRole
        {
            get { return targetData.obj; }
        }
        public Role role
        {
            get
            {
                if (targetData == null)
                    return this.owner;
                return this.targetData.obj;
            }
        }
    }
}