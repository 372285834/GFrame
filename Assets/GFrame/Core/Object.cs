using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
		private int _onlyId = -1;
        public int onlyId { get { return _onlyId; } }
		internal void SetOnlyId( int id ) { _onlyId = id; }
        public static T DeepCopyWithReflection<T>(T obj)
        {
            Type type = obj.GetType();

            // 如果是字符串或值类型则直接返回
            if (obj is string || type.IsValueType) return obj;
            if (type.IsArray)
            {
                Type elementType = Type.GetType(type.FullName.Replace("[]", string.Empty));
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(DeepCopyWithReflection(array.GetValue(i)), i);
                }

                return (T)Convert.ChangeType(copied, obj.GetType());
            }

            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fis = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fl in fis)
            {
                var propertyValue = fl.GetValue(obj);
                if (propertyValue == null)
                    continue;
                fl.SetValue(retval, DeepCopyWithReflection(propertyValue));
            }
            return (T)retval;
        }
    }
}
