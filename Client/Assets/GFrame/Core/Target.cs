using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
	public class Target
	{
        protected List<Role> mObjects;
        protected List<Vector3> mPositions;
        public Target()
        {
            mObjects = new List<Role>();
            mPositions = new List<Vector3>();
        }
        public bool checkIndex(int idx)
        {
            return idx >= 0 && idx < mObjects.Count;
        }
        public Vector3 getPos(int idx = 0)
        {
            return mPositions[idx];
        }
        public Role getObj(int idx = 0)
        {
            if (idx < 0 || idx >= mObjects.Count)
                return null;
            return mObjects[idx];
        }
        public void addPosition(Vector3 pos)
        {
            mPositions.Add(pos);
        }
        public void addObj(Role obj)
        {
            mObjects.Add(obj);
        }
        public void Clear()
        {
            mPositions.Clear();
            mObjects.Clear();
        }

    }

}
