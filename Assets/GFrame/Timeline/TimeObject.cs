using System.Collections;
using System.Collections.Generic;

namespace highlight.timeline
{
    public class TimeObject
    {
        public FrameRange frameRange { get; private set; }
        public Timeline root { get; private set; }
        public bool triggerOnSkip { get { return true; } }

        /// @brief Range of the event.
        public TimeStyle mStyle { get; private set; }
        public int id;
        public int owerId { get; private set; }

        // has this event called Trigger already?
        private bool _hasTriggered = false;
        /// @brief Has Trigger been called already?
        public bool HasTriggered { get { return _hasTriggered; } }

        // has this event called Finish already?
        private bool _hasFinished = false;
        /// @brief Has Finish been called already?
        public bool HasFinished { get { return _hasFinished; } }

        /// @brief At which frame will the event trigger, basically the start of it's range.
        public int TriggerFrame { get { return mStyle.Start; } }
        /// @brief At which time the event triggers.
        /// @note Value isn't cached.
        public float TriggerTime { get { return mStyle.Start * root.InverseFrameRate; } }
        private float mTimeSinceTrigger;
        public float progress { get { return mTimeSinceTrigger / LengthTime; } }
        public TimeObject mParent { get; private set; }
        public bool IsRoot { get { return mParent == null; } }
        /// @brief Get the tracks inside this timeline
        protected List<TimeObject> _childs = new List<TimeObject>();
        public List<TimeObject> GetChilds() { return _childs; }
        public List<TimeLogic> _components = new List<TimeLogic>();
        public List<TimeLogic> GetComponents { get { return _components; } }
        public TimeSystem system
        {
            get
            {
                return TimeSystem.Get(this.mStyle.eFlag);
            }
        }
        public static TimeObject Create(Timeline root, TimeStyle data, TimeObject parent)
        {
            if (root == null)
            {
                return null;
            }
            TimeObject obj = data.getObj();
            obj.mParent = parent;
            obj.root = root;
            obj.mStyle = data;
            obj.frameRange = data.Range;
            for (int i = 0; i < data.Components.Count; i++)
            {
                TimeLogic logic = TimeLogic.Get(data.Components[i], obj);
                obj._components.Add(logic);
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
            this.frameRange = this.mStyle.Range;
        }
        public TimeObject AddChild(TimeStyle data)
        {
            if (mStyle == null || data == null)
                return null;
            mStyle.Childs.Add(data);
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
            if (this.mStyle == s)
                return this;
            for (int i = 0; i < _childs.Count; i++)
            {
                TimeObject e = _childs[i].GetChild(s);
                if (e != null)
                    return e;
            }
            return null;
        }
        /// @brief Returns event on position \e index, they are ordered left to right.
        public void RemoveChild(TimeObject evt)
        {
            if (this.mStyle == null || evt == null)
                return;
            if (_childs.Remove(evt))
            {
                this.mStyle.Childs.Remove(evt.mStyle);
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
            mTimeSinceTrigger = 0f;
            for (int i = 0; i < _childs.Count; i++)
            {
                _childs[i].Init();
            }
            OnInit();
        }
        public void Destroy()
        {
            if (mStyle == null)
                return;
            for (int i = 0; i != _childs.Count; ++i)
                _childs[i].Destroy();
            OnDestroy();
            for (int i = 0; i < _components.Count; i++)
                TimeLogic.Release(_components[i]);
            _components.Clear();
            this.mStyle.release(this);
            this.mStyle = null;
            this.root = null;
            this.owerId = -1;
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
            _hasTriggered = false;
            _hasFinished = false;
            mTimeSinceTrigger = 0f;
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
        public void UpdateEvent(int framesSinceTrigger, float timeSinceTrigger)
        {
            if (HasFinished)
                return;
            mTimeSinceTrigger = timeSinceTrigger;
#if UNITY_EDITOR
            PreEvent();
#endif
            if (!_hasTriggered)
            {
                Trigger(framesSinceTrigger, timeSinceTrigger);
            }
            UpdateChilds(framesSinceTrigger, timeSinceTrigger);
            OnUpdateEvent(framesSinceTrigger, timeSinceTrigger);
            if (framesSinceTrigger == Length)
            {
                Finish();
            }
#if UNITY_EDITOR
            PostEvent();
#endif
        }
        public virtual void UpdateChilds(int frame, float time)
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
                    _childs[i].UpdateEvent(frame - _childs[i].Start, time - _childs[i].StartTime);
                }
                else //if( frame > _events[_currentEvent].End ) // is it finished
                {
                    if (!_childs[i].HasFinished && (_childs[i].HasTriggered || _childs[i].triggerOnSkip))
                    {
                        _childs[i].UpdateEvent(_childs[i].Length, _childs[i].LengthTime);
                    }
                }
            }
        }
        public void Trigger(int framesSinceTrigger, float timeSinceTrigger)
        {
            _hasTriggered = true;

            OnTrigger(framesSinceTrigger, timeSinceTrigger);
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
        /**
		 * @brief UpdateEvent but to only be called by the flux editor tools, should not be called at runtime.
		 */
        public void UpdateEventEditor(int framesSinceTrigger, float timeSinceTrigger)
        {
            mTimeSinceTrigger = timeSinceTrigger;
            PreEvent();

            if (!_hasTriggered)
                Trigger(framesSinceTrigger, timeSinceTrigger);

            OnUpdateEventEditor(framesSinceTrigger, timeSinceTrigger);
            UpdateChildsEditor(framesSinceTrigger, timeSinceTrigger);
            if (framesSinceTrigger == Length)
                Finish();

            PostEvent();
        }
        public virtual void UpdateChildsEditor(int frame, float time)
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
#if FLUX_PROFILER
				Profiler.BeginSample("Event: " + i + " " + _events[i].name );
#endif
                if (frame < _childs[i].Start)
                {
                    if (_childs[i].HasTriggered)
                        _childs[i].Stop();

                }
                else if (frame >= _childs[i].Start && frame <= _childs[i].End)
                {
                    if (_childs[i].HasFinished)
                        _childs[i].Stop();

                    _childs[i].UpdateEventEditor(frame - _childs[i].Start, time - _childs[i].StartTime);
                }
                else //if( currentFrame > _events[i].End )
                {
                    if (!_childs[i].HasFinished)
                        _childs[i].UpdateEventEditor(_childs[i].Length, _childs[i].LengthTime);

                }
#if FLUX_PROFILER
				Profiler.EndSample();
#endif
            }
        }
        protected virtual void OnUpdateEventEditor(int framesSinceTrigger, float timeSinceTrigger)
        {
            OnUpdateEvent(framesSinceTrigger, timeSinceTrigger);
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
        /// @brief Returns \e true if it is the last event of the track it belongs to.
		public bool IsLastEvent()
        {
            return id == root.GetChilds().Count - 1;
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

        /// @brief What this the event ends.
        /// @note This value isn't cached.
        public float EndTime
        {
            get { return mStyle.End * root.InverseFrameRate; }
        }

        /// @brief Length of the event in seconds.
        /// @note This value isn't cached.
        public float LengthTime
        {
            get { return mStyle.Length * root.InverseFrameRate; }
        }

        /// @brief What's the minimum length this event can have?
        /// @warning Events cannot be smaller than 1 frame.
        public virtual int GetMinLength()
        {
            return 1;
        }

        /// @brief What's the maximum length this event can have?
        public virtual int GetMaxLength()
        {
            return int.MaxValue;
        }
        public bool IsEmpty()
        {
            return _childs.Count == 0;
        }
        /// @brief Does the Event collides the \e e?
        public bool Collides(TimeObject e)
        {
            return frameRange.Collides(e.frameRange);
        }

        /// @brief Returns the biggest frame range this event can have
        public FrameRange GetMaxFrameRange()
        {
            //FrameRange range = new FrameRange(0, 0);
            if (this.mParent == null)
                return this.frameRange;
            else
                return this.mParent.frameRange;
        }

        /// @brief Compares events based on their start frame, basically used to order them.
        /// @param e1 Event
        /// @param e2 Event
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
            system.OnInit(this);
        }
        // timeline 销毁
        protected virtual void OnDestroy()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnDestroy();
            system.OnDestroy(this);
        }
        protected virtual void OnTrigger(int framesSinceTrigger, float timeSinceTrigger)
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnTrigger(framesSinceTrigger, timeSinceTrigger);
            system.OnTrigger(this, framesSinceTrigger, timeSinceTrigger);
        }
        protected virtual void OnUpdateEvent(int framesSinceTrigger, float timeSinceTrigger)
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnUpdateEvent(framesSinceTrigger, timeSinceTrigger);
            system.OnUpdateEvent(this, framesSinceTrigger, timeSinceTrigger);
        }
        // event完成
        protected virtual void OnFinish()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnFinish();
            system.OnFinish(this);
        }
        //timeline 完成
        protected virtual void OnStop()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnStop();
            system.OnStop(this);
        } 
        protected virtual void OnResume()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnResume();
            system.OnResume(this);
        }
        protected virtual void OnPause()
        {
            for (int i = 0; i < _components.Count; i++)
                _components[i].OnPause();
            system.OnPause(this);
        }
        #endregion
    }
}