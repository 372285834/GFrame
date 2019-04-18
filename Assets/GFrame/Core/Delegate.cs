using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace highlight
{
    public delegate void AcHandler();
    public delegate void AcHandler<T>(T obj);
    public delegate void AcHandler<T1, T2>(T1 arg1, T2 arg2);
    public delegate void AcHandler<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void AcHandler<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate T EvtHandler<T>(T obj);
}
