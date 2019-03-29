using System.Collections.Generic;

namespace highlight
{

    using UnityEngine;
    using ObjectList = List<SceneObject>;
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
        protected SceneObject obj;
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
        public void setObj(SceneObject _obj)
        {
            obj = _obj;
        }
        public SceneObject getObj()
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
