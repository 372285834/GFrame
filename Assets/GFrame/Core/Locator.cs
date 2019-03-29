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
            LT_MYSELF,
            LT_TARGET,
            LT_SCENE,
            LT_PARENT,
            LT_UI,
            LT_None
        }
        public enum eNameType
        {
            root,
        }
        public eType type;
        public eNameType eName;
        //[JsonIgnore]
        public Vector3 position;
       // [JsonIgnore]
        public Vector3 euler;
        public bool isFollow;
        public Locator(eType t, eNameType eN)
        {
            type = t;
            eName = eN;
            position = Vector3.zero;
            euler = Vector3.zero;
            isFollow = true;
        }
        public const string Root = "l_root";
        public const string HandR = "l_hand_r";
        public const string HandL = "l_hand_l";
        public const string WaistR = "l_waist_r";
        public const string WaistL = "l_waist_l";
        public const string Carry = "l_carry";
        public const string Back = "l_back";
        public const string Ride = "l_ride0";

        public object GetLocator()
        {
            switch (this.type)
            {
                case Locator.eType.LT_MYSELF:

                    break;
                case Locator.eType.LT_TARGET:

                    break;
                case Locator.eType.LT_SCENE:

                    break;
                case Locator.eType.LT_UI:
                    break;
            }
            return null;
        }
    }
}