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
    }

	public class Target
	{
        protected TargetType mType;
        protected Vector3 mPosition;
        protected ObjectList mObjects;

        public Target(TargetType type)
        {
            mType = type;
            mPosition = new Vector3();
            mObjects = new ObjectList();
        }

        public TargetType getType() { return mType; }

        public Vector3 getPosition()
        {
            switch (mType)
            {
                case TargetType.TT_Object:
                    {
                        if (mObjects.Count == 0)
                            return Vector3.zero;
                        else
                            return mObjects[0].getPosition();
                    }                
                case TargetType.TT_Point:
                    return mPosition;
            }

            return Vector3.zero;
        }

        public void setPosition(Vector3 pos)
        {
            mPosition = pos;
        }

        public ObjectList getObjectList() { return mObjects; }
        public void setObjectList(ObjectList list)
        {
            mObjects = list;
        }
	}

}
