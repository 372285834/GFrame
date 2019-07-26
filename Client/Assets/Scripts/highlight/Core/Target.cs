using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
	public class Target
	{
        public List<int> mObjects;
        public List<int> inObjects;
        public List<int> outObjects;

        public List<Vector3> mPositions;
        public Target()
        {
            mObjects = new List<int>();
            mPositions = new List<Vector3>();
            inObjects = new List<int>();
            outObjects = new List<int>();
        }
        public bool checkIndex(int idx)
        {
            return idx >= 0 && idx < mPositions.Count;
        }
        public Vector3 getPos(int idx = 0)
        {
            return mPositions[idx];
        }
        public Role getObj(int idx = 0)
        {
            if (idx < 0 || idx >= mObjects.Count)
                return null;
            int id = mObjects[idx];
            return RoleManager.Get(id);
        }
        public int GetDis(Vector3 pos,int idx = 0)
        {
            Role r = getObj(idx);
            if (r == null)
                return -1;
            float f = Vector3.Distance(pos, r.position) * 1000;
            return Mathf.RoundToInt(f);
        }
        public int GetDisPos(Vector3 pos, int idx = 0)
        {
            float f = Vector3.Distance(pos, mPositions[idx]) * 1000;
            return Mathf.RoundToInt(f);
        }
        public void setPosition(Vector3 pos)
        {
           // Debug.Log("setPosition" + pos);
            mPositions.Clear();
            addPosition(pos);
        }
        public void addPosition(Vector3 pos)
        {
            mPositions.Add(pos);
        }
        public void addObj(Role obj)
        {
            mObjects.Add(obj.onlyId);
        }
        public void setObj(Role obj)
        {
            mObjects.Clear();
            this.addObj(obj);
        }
        public void Clear()
        {
            mPositions.Clear();
            mObjects.Clear();
            inObjects.Clear();
            outObjects.Clear();
        }

    }

}
