using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace highlight
{
    /// <summary>
    /// 观察者;
    /// </summary>
    public class Observer
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
            if(isLock)
            {
                lockList.Add(_func);
                return;
            }
            for (int i = 0; i < mList.Count; i++)
            {
                if(_func == mList[i])
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
    }

    public class ObserverV<T>
    {
        //event AcHandler ac;
        protected List<EvtHandler<T>> mList = new List<EvtHandler<T>>();
        bool isLock = false;
         List<EvtHandler<T>> lockList = new List<EvtHandler<T>>();
        public virtual void AddObserver(EvtHandler<T> _func)
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
        public virtual void RemoveObserver(EvtHandler<T> _func)
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
        public virtual T Change(T t)
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

            t = OnChange(t);
            //if (ac != null)
            //    ac();
            isLock = false;
            return t;
        }
        public virtual T OnChange(T t)
        {
            for (int i = mList.Count - 1; i >= 0; i--)
            {
                t = mList[i](t);
            }
            return t;
        }
        public void Clear()
        {
            mList.Clear();
            lockList.Clear();
        }
    }
}
