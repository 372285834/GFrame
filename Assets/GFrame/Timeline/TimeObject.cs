using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.timeline
{
    public class TimeStyle
    {
        public int Start = 0;
        public int End = 0;
        //[NonSerialized]
        public List<TimeStyle> Childs = new List<TimeStyle>();
        public List<ComponentStyle> Components = new List<ComponentStyle>();
        public List<ActionStyle> Actions = new List<ActionStyle>();
        // [JsonIgnore]
        public int Length { set { End = Start + value; } get { return End - Start; } }
        // [JsonIgnore]
        public FrameRange Range
        {
            get { return new FrameRange(Start, End); }
            set { Start = value.Start; End = value.End; }
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

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
    public class TimeObject
    {
        public FrameRange frameRange { get; private set; }
        public Timeline root { get; private set; }
        public bool triggerOnSkip { get { return true; } }

        public TimeStyle timeStyle { get; private set; }
        public ResData resData { get; private set; }
        public int id;
        public bool activeSelf { get; private set; }
        public void SetActive(bool v)
        {
            activeSelf = v;
            if (this.mFrameSinceTrigger > 0)
            {
                this.update(this.mFrameSinceTrigger);
            }
        }
        private bool _hasTriggered = false;
        public bool HasTriggered { get { return _hasTriggered; } }

        private bool _hasFinished = false;
        public bool HasFinished { get { return _hasFinished; } }
        public float progress { get; private set; }

        private int mFrameSinceTrigger;
        public int frameSinceTrigger { get { return mFrameSinceTrigger; } }
        public float timeSinceTrigger { get { return mFrameSinceTrigger * root.InverseFrameRate; } }
        public TimeObject parent { get; private set; }
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
            {
                return null;
            }
            TimeObject obj = data.getObj();
            obj.parent = parent;
            obj.root = root;
            obj.timeStyle = data;
            obj.frameRange = data.Range;
            for (int i = 0; i < data.Components.Count; i++)
            {
                ComponentData comp = ComponentData.Get(data.Components[i], obj);
                if (comp is ResData)
                {
                    obj.resData = comp as ResData;
                }
                obj._components.Add(comp);
            }
            for (int i = 0; i < data.Actions.Count;i++ )
            {
                TimeAction action = TimeAction.Get(data.Actions[i], obj) as TimeAction;
                obj._actions.Add(action);
            }
            for (int i = 0; i < data.Childs.Count; i++)
            {
                TimeObject child = TimeObject.Create(root, data.Childs[i], obj);
                child.id = i;
                obj._childs.Add(child);
            }
            return obj;
        }
        public virtual void OnStyleChange()
        {
            this.frameRange = this.timeStyle.Range;
        }
        public TimeObject AddChild(TimeStyle data)
        {
            if (timeStyle == null || data == null)
                return null;
            timeStyle.Childs.Add(data);
            TimeObject evt = TimeObject.Create(this.root, data, this);
            int id = _childs.Count;
            _childs.Add(evt);
            evt.id = id;
            evt.Init();
            return evt;
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
        public void RemoveChild(TimeObject evt)
        {
            if (this.timeStyle == null || evt == null)
                return;
            if (_childs.Remove(evt))
            {
                this.timeStyle.Childs.Remove(evt.timeStyle);
                evt.Destroy();
                UpdateChildIds();
            }
        }
        public void Rebuild()
        {
            UpdateChildIds();
        }
        private void UpdateChildIds()
        {
            for (int i = 0; i != _childs.Count; ++i)
                _childs[i].id = i;
        }

        public void Init()
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
        public void Destroy()
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

        public void Pause()
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

        public void Resume()
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

        public void Stop()
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
            progress = (float)mFrameSinceTrigger / this.Length;
        }
        protected void update(int frame)
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
                    _childs[i].update(frame - _childs[i].Start);
                }
                else //if( frame > _events[_currentEvent].End ) // is it finished
                {
                    if (!_childs[i].HasFinished && (_childs[i].HasTriggered || _childs[i].triggerOnSkip))
                    {
                        _childs[i].update(_childs[i].Length);
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
        public void UpdateEditor(int frame)
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
        public virtual void UpdateChildsEditor(int frame)
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
            return id == root.Childs.Count - 1;
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
            get { return frameRange.Start * root.InverseFrameRate; }
        }
        public float EndTime
        {
            get { return frameRange.End * root.InverseFrameRate; }
        }
        public float LengthTime
        {
            get { return frameRange.Length * root.InverseFrameRate; }
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
        public static int Compare(TimeObject e1, TimeObject e2)
        {
            return e1.Start.CompareTo(e2.Start);
        }

        #region virtual Function
        protected virtual void OnInit()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].index = i;
                _components[i].OnInit();
            }
            for (int i = 0; i < _actions.Count; i++)
            {
                _actions[i].index = i;
                _actions[i].OnInit();
            }
        }
        // timeline 销毁
        protected virtual void OnDestroy()
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
        protected virtual void OnTrigger()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnTrigger();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnTrigger();
        }
        protected virtual void OnUpdate()
        {
            //for (int i = 0; i < _components.Count; i++)
            //    _components[i].OnUpdate();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnUpdate();
        }
        // event完成
        protected virtual void OnFinish()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnFinish();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnFinish();
        }
        //timeline 完成
        protected virtual void OnStop()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnStop();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnStop();
        } 
        protected virtual void OnResume()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnResume();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnResume();
        }
        protected virtual void OnPause()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnPause();
            for (int i = 0; i < _actions.Count; i++)
                _actions[i].OnPause();
        }
        #endregion
    }
}