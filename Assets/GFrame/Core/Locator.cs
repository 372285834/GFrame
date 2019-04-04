using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    public struct Locator
    {
        public enum eType
        {
            LT_OWNER,
            LT_TARGET,
            LT_TARGET_POS,
            LT_SCENE,
            LT_PARENT,
            LT_PARENT_POS,
            //LT_UI,
            //LT_None
        }
        public enum eNameType
        {
            ROOT,
        }
        public eType type;
        public eNameType eName;
        //[JsonIgnore]
        public Locator(eType t, eNameType eN)
        {
            type = t;
            eName = eN;
        }
        public const string Root = "l_root";
        public const string HandR = "l_hand_r";
        public const string HandL = "l_hand_l";
        public const string WaistR = "l_waist_r";
        public const string WaistL = "l_waist_l";
        public const string Carry = "l_carry";
        public const string Back = "l_back";
        public const string Ride = "l_ride0";
        public string parentName
        {
            get
            {
                return eName.ToString();
            }
        }

    }
}