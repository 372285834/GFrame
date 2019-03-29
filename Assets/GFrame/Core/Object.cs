using System.Collections;
using System.Collections.Generic;

namespace highlight
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
    public abstract class Object
	{
		private int _id = -1;
        public int id { get { return _id; } }
		internal void SetId( int id ) { _id = id; }
	}
}
