using System;
using System.Collections.Generic;
using UnityEngine;
public class GameObjectPool<T> where T : Component
{
    private readonly Stack<T> m_Stack = new Stack<T>();
    private readonly Action<T> m_ActionOnGet;
    private readonly Action<T> m_ActionOnRelease;
    public GameObject Temp;
    public int countAll { get; private set; }
    public int countActive { get { return countAll - countInactive; } }
    public int countInactive { get { return m_Stack.Count; } }

    public GameObjectPool(GameObject _go, Action<T> actionOnGet, Action<T> actionOnRelease, Transform parent = null)
    {
        Temp = _go;
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        mParent = parent;
    }
    public Transform mParent;
    public T Get()
    {
        return Get(mParent);
    }
    public T Get(Transform parent)
    {
        T element;
        if (m_Stack.Count == 0)
        {
            GameObject go = GameObject.Instantiate(Temp, parent);
            //go.name = Temp.name + "_" + countAll;
            element = go.GetComponent<T>();
            if (element == null)
                element = go.AddComponent<T>();
            countAll++;
        }
        else
        {
            element = m_Stack.Pop();
        }
        if (m_ActionOnGet != null)
            m_ActionOnGet(element);
        return element;
    }

    public void Release(T element)
    {
        //if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
        //    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
        if (m_ActionOnRelease != null)
            m_ActionOnRelease(element);
        m_Stack.Push(element);
    }
}