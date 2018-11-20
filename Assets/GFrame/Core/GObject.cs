using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GP
{
    public class Id
    {
        protected ulong mCurrentId;

        public Id(ulong start = 0)
        {
            mCurrentId = start;
        }

        //This function assumes creation of new objects can't be made from multiple threads!!!
        public ulong generateNewId()
        {
            return mCurrentId++;
        }
    };
    public abstract class GObject
	{
		private int _id = -1;
        public int id { get { return _id; } }
		internal void SetId( int id ) { _id = id; }
	}

    public class GSceneObject : GObject
    {
        public Transform transform;
        public Dictionary<string, Transform> LocatorDic = new Dictionary<string, Transform>();
        //public AnimationBox aniBox;
        public Vector3 getPosition()
        {
            return transform.position;
        }

        public Transform getLocator(string name)
        {
            return null;
        }
        public void PlayAction()
        {

        }
    }
}
