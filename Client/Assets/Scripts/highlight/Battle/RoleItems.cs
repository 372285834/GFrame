using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    public class RoleItems : Object
    {
        public Role role;
        public int weaponId;


        public bool SwitchWeapon(int id)
        {
            if (weaponId == id)
                return false;
            weaponId = id;
            return true;
        }
    }
}