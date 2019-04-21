using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace highlight
{
    public interface IObserver
    {
        void Clear();
    }
    /// <summary>
    /// 观察者;
    /// </summary>
    public class Observer : IObserver
    {
        //event AcHandler ac;
        List<AcHandler> mList = new List<AcHandler>();
        bool isLock = false;
        List<AcHandler> lockList = new List<AcHandler>();
        public virtual void AddObserver(AcHandler _func, bool immediately = true)
        {
            if (immediately)
                _func();
            for (int i = 0; i < lockList.Count; i++)
            {
                if (_func == lockList[i])
                {
                    lockList.RemoveAt(i);
                    return;
                }
            }
            for (int i = 0; i < mList.Count; i++)
            {
                if (_func == mList[i])
                {
                    return;
                }
            }
            mList.Add(_func);
            //ac -= _func;
            //ac += _func;
        }
        public virtual void RemoveObserver(AcHandler _func)
        {
            if (isLock)
            {
                lockList.Add(_func);
                return;
            }
            for (int i = 0; i < mList.Count; i++)
            {
                if (_func == mList[i])
                {
                    mList.RemoveAt(i);
                    return;
                }
            }
            //int idx = mList.FindIndex(x => x == _func);
            //if (idx > -1)
            //{
            //    mList.RemoveAt(idx);
            //}
            //ac -= _func;
        }
        public virtual void Change()
        {
            isLock = true;
            if (lockList.Count > 0)
            {
                for (int j = 0; j < lockList.Count; j++)
                {
                    for (int i = mList.Count - 1; i >= 0; i--)
                    {
                        if (lockList[j] == mList[i])
                            mList.RemoveAt(i);
                    }
                }
                lockList.Clear();
            }

            for (int i = mList.Count - 1; i >= 0; i--)
            {
                mList[i]();
            }
            //if (ac != null)
            //    ac();
            isLock = false;
        }
        public void Clear()
        {
            mList.Clear();
            lockList.Clear();
        }
    }

    public class ObserverV<T> : IObserver
    {
        //event AcHandler ac;
        protected List<AcHandler<T>> mList = new List<AcHandler<T>>();
        bool isLock = false;
        List<AcHandler<T>> lockList = new List<AcHandler<T>>();
        public virtual void AddObserver(AcHandler<T> _func)
        {
            for (int i = 0; i < lockList.Count; i++)
            {
                if (_func == lockList[i])
                {
                    lockList.RemoveAt(i);
                    return;
                }
            }
            for (int i = 0; i < mList.Count; i++)
            {
                if (_func == mList[i])
                {
                    return;
                }
            }
            mList.Add(_func);
            //ac -= _func;
            //ac += _func;
        }
        public virtual void RemoveObserver(AcHandler<T> _func)
        {
            if (isLock)
            {
                lockList.Add(_func);
                return;
            }
            for (int i = 0; i < mList.Count; i++)
            {
                if (_func == mList[i])
                {
                    mList.RemoveAt(i);
                    return;
                }
            }
            //int idx = mList.FindIndex(x => x == _func);
            //if (idx > -1)
            //{
            //    mList.RemoveAt(idx);
            //}
            //ac -= _func;
        }
        public void Change(T t)
        {
            isLock = true;
            if (lockList.Count > 0)
            {
                for (int j = 0; j < lockList.Count; j++)
                {
                    for (int i = mList.Count - 1; i >= 0; i--)
                    {
                        if (lockList[j] == mList[i])
                            mList.RemoveAt(i);
                    }
                }
                lockList.Clear();
            }

            for (int i = mList.Count - 1; i >= 0; i--)
            {
                mList[i](t);
            }
            //if (ac != null)
            //    ac();
            isLock = false;
        }
        public void Clear()
        {
            mList.Clear();
            lockList.Clear();
        }
    }
}
