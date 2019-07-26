using highlight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class XLuaList : MList
    {
        public XLua.LuaTable luaData;
        protected override void OnDestroy()
        {
            if (luaData != null)
            {
                luaData.Dispose();
                luaData = null;
            }
            base.OnDestroy();
        }
    }
