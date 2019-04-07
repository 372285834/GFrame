using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace highlight.timeline
{
    public class TimeStyle : Object
    {
        public string name = "";
        public int x = 0;
        public int y = 0;
        //[NonSerialized]
        public TimeStyle[] Childs = new TimeStyle[0];
        public ComponentStyle[] Components = new ComponentStyle[0];
        public ActionStyle[] Actions = new ActionStyle[0];
        // [JsonIgnore]
        public int Length { set { y = x + value; } get { return y - x; } }
        // [JsonIgnore]
        public FrameRange Range
        {
            get { return new FrameRange(x, y); }
            set { x = value.Start; y = value.End; }
        }

        //public object Clone(bool deep = true)
        //{
        //    if (deep)
        //        return Object.DeepCopyWithReflection<TimeStyle>(this);
        //    return this.MemberwiseClone();
        //}

        public static ObjectPool<TimeObject> objPool = new ObjectPool<TimeObject>();
        public virtual TimeObject getObj()
        {
            TimeObject obj = objPool.Get();
            return obj;
        }
        public virtual void release(TimeObject obj)
        {
            objPool.Release(obj);
        }
    }
    public class TimeObject : Object
    {
        public int index = 0;
        public FrameRange frameRange { get; private set; }
        public Timeline root { get; private set; }
        public bool triggerOnSkip { get { return true; } }

        public TimeStyle timeStyle { get; private set; }
        public ResData resData { get; private set; }
        public bool activeSelf { get; private set; }
        private bool _hasTriggered = false;
        public bool HasTriggered { get { return _hasTriggered; } }

        private bool _hasFinished = false;
        public bool HasFinished { get { return _hasFinished; } }
        public float progress { get; private set; }

        private int mFrameSinceTrigger;
        public int frameSinceTrigger { get { return mFrameSinceTrigger; } }
        public float timeSinceTrigger { get { return mFrameSinceTrigger / root.FrameRate; } }
        public TimeObject parent { get; private set; }
        public int Depth
        {
            get
            {
                int dep = 0;
                var _parent = this.parent;
                while (_parent != null)
                {
                    dep++;
                    _parent = _parent.parent;
                }
                return dep;
            }
        }
        public bool IsRoot { get { return parent == null; } }
        protected List<TimeObject> _childs = new List<TimeObject>();
        public List<TimeObject> Childs { get { return _childs; } }
        List<ComponentData> _components = new List<ComponentData>();
        public List<ComponentData> GetComponents { get { return _components; } }
        List<TimeAction> _actions = new List<TimeAction>();
        public List<TimeAction> Actions { get { return _actions; } }
        public ComponentData GetComponent(int idx)
        {
            if (idx < 0 || idx >= _components.Count)
                return null;
            return _components[idx];
        }
        public static TimeObject Create(Timeline root, TimeStyle data, TimeObject parent)
        {
            if (root == null)
                return null;
            TimeObject obj = null;
            if (parent == null)
                obj = root;
            else
                obj = data.getObj();
            obj.parent = parent;
            obj.root = root;
            obj.timeStyle = data;
            obj.frameRange = data.Range;
            root.nodeDic[data.name] = obj;
            for (int i = 0; i < data.Components.Length; i++)
            {
                ComponentData comp = ComponentData.Get(data.Components[i], obj);
                comp.index = i;
                obj._components.Add(comp);
                if (comp is ResData)
                    obj.resData = comp as ResData;
            }
            for (int i = 0; i < data.Actions.Length; i++)
            {
                ActionStyle style = data.Actions[i];
                TimeAction action = TimeAction.Get(style, obj) as TimeAction;
                action.index = i;
                obj._actions.Add(action);
                action.SetData(obj._components);
            }
            for (int i = 0; i < data.Childs.Length; i++)
            {
                TimeObject child = TimeObject.Create(root, data.Childs[i], obj);
                child.index = i;
                obj._childs.Add(child);
            }
            return obj;
        }
        public virtual void OnStyleChange()
        {
            this.frameRange = this.timeStyle.Range;
        }
        public TimeObject GetChild(int index)
        {
            return _childs[index];
        }
        public TimeObject GetChild(TimeStyle s)
        {
            if (this.timeStyle == s)
                return this;
            for (int i = 0; i < _childs.Count; i++)
            {
                TimeObject e = _childs[i].GetChild(s);
                if (e != null)
                    return e;
            }
            return null;
        }
        public TimeObject AddChild(TimeStyle data)
        {
            if (timeStyle == null || data == null)
                return null;
            AddArray<TimeStyle>(ref timeStyle.Childs, data);
            //timeStyle.Childs.Add(data);
            TimeObject obj = TimeObject.Create(this.root, data, this);
            obj.index = _childs.Count;
            _childs.Add(obj);
            obj.Init();
            return obj;
        }
        public void RemoveChild(TimeObject evt)
        {
            if (this.timeStyle == null || evt == null)
                return;
            evt.Destroy();
            if (_childs.Remove(evt))
            {
                RemoveArray<TimeStyle>(ref timeStyle.Childs, evt.timeStyle);
                UpdateChildIds();
            }
        }
        public void SetChildIndex(TimeObject obj, int idx)
        {
            this._childs.Remove(obj);
            ArrayMove<TimeStyle>(ref this.timeStyle.Childs, obj.index, idx);
           // idx = idx > obj.index ? idx - 1 : idx;
            this._childs.Insert(idx, obj);
            UpdateChildIds();
        }
        public void AddComponent(ComponentStyle style)
        {
            AddArray<ComponentStyle>(ref this.timeStyle.Components, style);
            ComponentData comp = ComponentData.Get(style, this);

            this._components.Add(comp);
            comp.index = this._components.Count - 1;
            if (comp is ResData)
                this.resData = comp as ResData;
        }
        public void RemoveComponent(ComponentData data)
        {
            data.OnDestroy();
            RemoveArray<ComponentStyle>(ref this.timeStyle.Components, data.style);
            this._components.Remove(data);
            for(int i=0;i<this.timeStyle.Actions.Length;i++)
            {
                int[] indexs = timeStyle.Actions[i].Indexs;
                for(int j=0;j<indexs.Length;j++)
                {
                    if (indexs[j] == data.index)
                        indexs[j] = -1;
                }
            }
            Rebuild();
        }
        public void SetComponentIndex(ComponentData data, int idx)
        {
            this._components.Remove(data);
            ArrayMove<ComponentStyle>(ref this.timeStyle.Components, data.index, idx);
         //   idx = idx > data.index ? idx - 1 : idx;
            this._components.Insert(idx, data);
            for (int k = 0; k < this.timeStyle.Actions.Length; k++)
            {
                int[] indexs = timeStyle.Actions[k].Indexs;
                for (int j = 0; j < indexs.Length; j++)
                {
                    for (int i = 0; i < _components.Count; i++)
                    {
                        if (indexs[j] == _components[i].index)
                        {
                            indexs[j] = i;
                            break;
                        }
                    }
                }
            }
            Rebuild();
        }
        public void AddAction(ActionStyle actionStyle)
        {
            AddArray<ActionStyle>(ref this.timeStyle.Actions, actionStyle);
            TimeAction action = TimeAction.Get(actionStyle, this) as TimeAction;
            this._actions.Add(action);
            action.index = this._actions.Count - 1;
            action.SetData(this._components);
        }
        public void RemoveAction(TimeAction action)
        {
            action.OnDestroy();
            RemoveArray<ActionStyle>(ref this.timeStyle.Actions, action.style);
            this._actions.Remove(action);
            Rebuild();
        }
        public void SetActionIndex(TimeAction action, int idx)
        {
            this._actions.Remove(action);
            ArrayMove<ActionStyle>(ref this.timeStyle.Actions, action.index, idx);
         //   idx = idx > action.index ? idx - 1 : idx;
            this._actions.Insert(idx, action);
            Rebuild();
        }
        public void Rebuild()
        {
            UpdateChildIds();
            for (int i = 0; i < _components.Count; i++)
                _components[i].index = i;
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].index = i;
        }
        private void UpdateChildIds()
        {
            for (int i = 0; i < _childs.Count; ++i)
                _childs[i].SetId(i);
        }

        public virtual void Init()
        {
            _hasTriggered = false;
            _hasFinished = false;
            setProgress(0);
            OnInit();
            for (int i = 0; i < _childs.Count; i++)
            {
                _childs[i].Init();
            }
        }
        public virtual void Destroy()
        {
            if (timeStyle == null)
                return;
            for (int i = 0; i != _childs.Count; ++i)
                _childs[i].Destroy();
            OnDestroy();
            this.timeStyle.release(this);
            this.timeStyle = null;
            this.resData = null;
            this.root = null;
            _childs.Clear();
        }

        public virtual void Pause()
        {
#if UNITY_EDITOR
            PreEvent();
#endif
            for (int i = 0; i != _childs.Count; ++i)
            {
                if (_childs[i].HasTriggered && !_childs[i].HasFinished)
                    _childs[i].Pause();
            }
            OnPause();
#if UNITY_EDITOR
            PostEvent();
#endif
        }

        public virtual void Resume()
        {
#if UNITY_EDITOR
            PreEvent();
#endif
            for (int i = 0; i != _childs.Count; ++i)
            {
                if (_childs[i].HasTriggered && !_childs[i].HasFinished)
                    _childs[i].Resume();
            }
            OnResume();
#if UNITY_EDITOR
            PostEvent();
#endif
        }

        public virtual void Stop()
        {
            //_hasTriggered = true;
            _hasFinished = true;
            setProgress(0);
#if UNITY_EDITOR
            PreEvent();
#endif
            for (int i = _childs.Count - 1; i >= 0; --i)
            {
                if (_childs[i].HasTriggered)
                    _childs[i].Stop();
            }
            OnStop();

#if UNITY_EDITOR
            PostEvent();
#endif
        }
        void setProgress(int framesSinceTrigger)
        {
            mFrameSinceTrigger = framesSinceTrigger;
            if (this.Length <= 0)
                progress = 1f;
            else
                progress = (float)mFrameSinceTrigger / this.Length;
        }
        protected void UpdateFrame(int frame)
        {
            setProgress(frame);
            if (!activeSelf || HasFinished)
                return;
#if UNITY_EDITOR
            PreEvent();
#endif
            if (!_hasTriggered)
            {
                Trigger();
            }
            OnUpdate();
            UpdateChilds(frame);
            if (frame == Length)
            {
                Finish();
            }
#if UNITY_EDITOR
            PostEvent();
#endif
        }
        public virtual void UpdateChilds(int frame)
        {
            int limit = _childs.Count;
            int increment = 1;

            if (!root.IsPlayingForward)
            {
                limit = -1;
                increment = -1;
            }
            for (int i = 0; i != limit; i += increment)
            {
                if (frame < _childs[i].Start)
                {
                }
                else if (frame >= _childs[i].Start && frame <= _childs[i].End)
                {
                    _childs[i].UpdateFrame(frame - _childs[i].Start);
                }
                else //if( frame > _events[_currentEvent].End ) // is it finished
                {
                    if (!_childs[i].HasFinished && (_childs[i].HasTriggered || _childs[i].triggerOnSkip))
                    {
                        _childs[i].UpdateFrame(_childs[i].Length);
                    }
                }
            }
        }
        public void Trigger()
        {
            _hasTriggered = true;

            OnTrigger();
        }
        public void Finish()
        {
            _hasFinished = true;
#if UNITY_EDITOR
            PreEvent();
#endif
            OnFinish();
#if UNITY_EDITOR
            PostEvent();
#endif
        }
        #region editor
#if UNITY_EDITOR
        protected void UpdateEditor(int frame)
        {
            setProgress(frame);
            PreEvent();

            if (!_hasTriggered)
                Trigger();

            OnUpdateEditor();
            UpdateChildsEditor(frame);
            if (frame == Length)
                Finish();

            PostEvent();
        }
        protected virtual void UpdateChildsEditor(int frame)
        {
            int limit = _childs.Count;

            if (limit == 0)
                return;

            int increment = 1;

            if (!root.IsPlayingForward)
            {
                limit = -1;
                increment = -1;
            }

            for (int i = 0; i != limit; i += increment)
            {
                //Profiler.BeginSample("Event: " + i + " " + _events[i].name );
                if (frame < _childs[i].Start)
                {
                    if (_childs[i].HasTriggered)
                        _childs[i].Stop();

                }
                else if (frame >= _childs[i].Start && frame <= _childs[i].End)
                {
                    if (_childs[i].HasFinished)
                        _childs[i].Stop();

                    _childs[i].UpdateEditor(frame - _childs[i].Start);
                }
                else //if( currentFrame > _events[i].End )
                {
                    if (!_childs[i].HasFinished)
                        _childs[i].UpdateEditor(_childs[i].Length);

                }
                //Profiler.EndSample();
            }
        }
        protected virtual void OnUpdateEditor()
        {
            OnUpdate();
        }
        protected virtual void PreEvent()
        {
            //Owner.gameObject.hideFlags = HideFlags.DontSave;
        }
        protected virtual void PostEvent()
        {
            //Owner.gameObject.hideFlags = HideFlags.None;
        }
#endif
        #endregion
        public bool IsLastEvent()
        {
            return index == root.Childs.Count - 1;
        }
        public int Start
        {
            get { return frameRange.Start; }
        }

        public int End
        {
            get { return frameRange.End; }
        }

        public int Length
        {
            get { return frameRange.Length; }
        }
        public float StartTime
        {
            get { return frameRange.Start / root.FrameRate; }
        }
        public float EndTime
        {
            get { return frameRange.End / root.FrameRate; }
        }
        public float LengthTime
        {
            get { return frameRange.Length / root.FrameRate; }
        }
        public virtual int GetMinLength()
        {
            return 1;
        }
        public virtual int GetMaxLength()
        {
            return int.MaxValue;
        }
        public bool IsEmpty()
        {
            return _childs.Count == 0;
        }
        public bool Collides(TimeObject e)
        {
            return frameRange.Collides(e.frameRange);
        }

        public FrameRange GetMaxFrameRange()
        {
            //FrameRange range = new FrameRange(0, 0);
            if (this.parent == null)
                return this.frameRange;
            else
                return this.parent.frameRange;
        }

        public int AllCount
        {
            get
            {
                int count = 0;
                for(int i=0;i<Childs.Count;i++)
                {
                    count += Childs[i].AllCount + 1;
                }
                return count;
            }
        }
        public static int Compare(TimeObject e1, TimeObject e2)
        {
            return e1.Start.CompareTo(e2.Start);
        }

        #region virtual Function
        protected void OnInit()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].OnInit();
            }
            for (int i = 0; i < _actions.Count; i++)
            {
                _actions[i].OnInit();
            }
        }
        // timeline 销毁
        protected void OnDestroy()
        {
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnDestroy();
            for (int i = 0; i < _components.Count; i++)
            {
                ComponentData.Release(_components[i]);
                _components[i].OnDestroy();
            }
            _components.Clear();
        }
        protected void OnTrigger()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnTrigger();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnTrigger();
        }
        protected void OnUpdate()
        {
            //for (int i = 0; i < _components.Count; i++)
            //    _components[i].OnUpdate();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnUpdate();
        }
        // event完成
        protected void OnFinish()
        {
          //  for (int i = 0; i < _components.Count; i++)
         //       _components[i].OnFinish();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnFinish();
        }
        //timeline 完成
        protected void OnStop()
        {
         //   for (int i = 0; i < _components.Count; i++)
         //       _components[i].OnStop();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnStop();
        }
        protected void OnResume()
        {
          //  for (int i = 0; i < _components.Count; i++)
          //      _components[i].OnResume();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnResume();
        }
        protected void OnPause()
        {
         //   for (int i = 0; i < _components.Count; i++)
         //       _components[i].OnPause();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnPause();
        }
        #endregion


        public static void AddArray<T>(ref T[] array, T t) where T : Object
        {
            int count = array.Length;
            Array.Resize<T>(ref array, count + 1);
            array[count] = t;
        }
        //public static int FindIndex<T>(T[] array, Predicate<T> match);
        public static int RemoveArray<T>(ref T[] array, T t) where T : Object
        {
            //  int idx = Array.FindIndex<T>(array, x => x == t);
            //  if (idx > -1)
            //  {
            int idx = -1;
            int count = array.Length;
            T[] arr = new T[count - 1];
            int num = 0;
            for (int i = 0; i < count; i++)
            {
                if (array[i] == t)
                {
                    idx = i;
                    continue;
                }
                arr[num] = array[i];
                num++;
            }
            array = arr;
            return idx;
            //   }
            //  return array;
        }
        /// <summary>
        /// 将数组的某一索引位置的元素移动到指定索引位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="fromIndex">要移动的元素所在的索引位</param>
        /// <param name="toIndex">要移动到的索引位</param>
        public static void ArrayMove<T>(ref T[] array, int fromIndex, int toIndex)
        {
            if (fromIndex > array.Length - 1 || toIndex > array.Length - 1 || fromIndex == toIndex) return;

            T[] tempArray = new T[array.Length];
            if (fromIndex > toIndex)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == toIndex)
                    {
                        tempArray[i] = array[fromIndex];
                    }
                    else
                    {
                        if (i > fromIndex || i < toIndex)
                        {
                            tempArray[i] = array[i];
                        }
                        else
                        {
                            tempArray[i] = array[i - 1];
                        }
                    }
                }
            }
            else if (fromIndex < toIndex)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i == toIndex)
                    {
                        tempArray[i] = array[fromIndex];
                    }
                    else
                    {
                        if (i < fromIndex || i > toIndex)
                        {
                            tempArray[i] = array[i];
                        }
                        else
                        {
                            tempArray[i] = array[i + 1];
                        }
                    }
                }
            }
            array = tempArray;
        }
    }
}