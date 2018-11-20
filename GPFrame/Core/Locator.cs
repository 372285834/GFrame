using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GP
{
    [Serializable]
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
        public Vector3 position;
        public Quaternion rotation;
        public bool isFollow;
        public Locator(eType t, eNameType eN)
        {
            type = t;
            eName = eN;
            position = Vector3.zero;
            rotation = Quaternion.identity;
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
    }
}