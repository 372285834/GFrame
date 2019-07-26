using highlight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public static class LuaBattle
{

    public static Role CreatRole(RoleType t,LuaTable excel,LuaTable brithData)
    {
        Role r = RoleManager.Creat(t);
        return r;
    }
}
