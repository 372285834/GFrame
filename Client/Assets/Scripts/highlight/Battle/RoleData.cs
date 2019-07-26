using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    public partial class Role
    {
        public NpcMapData npcMapData
        {
            get
            {
                return this.data as NpcMapData;
            }
        }
    }
}