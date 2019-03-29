using System.Collections.Generic;

namespace GP
{

    using UnityEngine;
    using ObjectList = List<GSceneObject>;
    public enum TargetType
    {
        TT_None,
        TT_Point,
        TT_Object,
        TT_ObjectList,
    }

	public class Target
	{
        protected TargetType mType;
        protected Vector3 mPosition;
        protected GSceneObject obj;
        protected ObjectList mObjects;

        public Target(TargetType type)
        {
            mType = type;
            mPosition = Vector3.zero;
        }

        public TargetType getType() { return mType; }

        public Vector3 getPosition()
        {
            Vector3 pos = Vector3.zero;
            switch (mType)
            {
                case TargetType.TT_Point:
                    pos = mPosition;
                    break;
                case TargetType.TT_Object:
                    if (obj != null)
                        pos = obj.getPosition();
                    break;
                case TargetType.TT_ObjectList:
                    if (mObjects != null && mObjects.Count > 0)
                        pos = mObjects[0].getPosition();
                    break;
            }
            return pos;
        }
        public void setPosition(Vector3 pos)
        {
            mPosition = pos;
        }
        public void setObj(GSceneObject _obj)
        {
            obj = _obj;
        }
        public GSceneObject getObj()
        {
            return obj;
        }
        public ObjectList getObjectList()
        {
            return mObjects;
        }
        public void setObjectList(ObjectList list)
        {
            mObjects = list;
        }
        public void Clear()
        {

        }

    }

}
