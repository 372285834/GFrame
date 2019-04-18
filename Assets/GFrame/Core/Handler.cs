using UnityEngine;
using System.Collections;
using System;

namespace highlight
{
    
    public interface IHandler
    {
        void Exc();
    }
    public class Handler : IHandler
    {
        AcHandler func;

        public Handler(AcHandler _func)
        {
            func = _func;
        }

        public void Exc()
        {
            if (func != null)
                func();
        }
    }

    public class Handler<T> : IHandler
    {
        AcHandler<T> func;
        T p1;

        public Handler(AcHandler<T> _func, T _p1)
        {
            func = _func;
            p1 = _p1;
        }

        public void Exc()
        {
            if (func != null)
                func(p1);
        }
    }

    public class Handler<T, T2> : IHandler
    {
        AcHandler<T, T2> func;
        T p1;
        T2 p2;

        public Handler(AcHandler<T, T2> _func, T _p1, T2 _p2)
        {
            func = _func;
            p1 = _p1;
            p2 = _p2;
        }

        public void Exc()
        {
            if (func != null)
                func(p1, p2);
        }
    }
    public class Handler<T, T2, T3> : IHandler
    {
        AcHandler<T, T2, T3> func;
        T p1;
        T2 p2;
        T3 p3;
        public Handler(AcHandler<T, T2, T3> _func, T _p1, T2 _p2, T3 _p3)
        {
            func = _func;
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
        }

        public void Exc()
        {
            if (func != null)
                func(p1, p2, p3);
        }
    }
    public class CHandler<T>
    {
        AcHandler<T> func;

        public CHandler(AcHandler<T> _func)
        {
            func = _func;
        }

        public void Exc(T t)
        {
            if (func != null)
                func(t);
        }
    }
}
