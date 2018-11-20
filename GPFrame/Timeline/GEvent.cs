using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GP
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GEventAttribute : System.Attribute
    {
        public string menu;
        public Type dataType;
        public int capacity;
        public GEventAttribute(string menu,Type dType)
            : this(menu,100, dType)
        {
        }
        public GEventAttribute(string _menu,int _capacity, Type dType)
        {
            this.menu = _menu;
            capacity = _capacity;
            this.dataType = dType;
        }
    }
    public abstract class GEventStyle
    {
        //public bool TriggerOnSkip = true;
        public int Start;
        public int End;
        public List<string> jsons = new List<string>();
        public List<string> types = new List<string>();
        [NonSerialized]
        public List<GEventStyle> styles = new List<GEventStyle>();
        public int Length { set { End = Start + value; } get { return End - Start; } }
        public FrameRange range {
            get { return new FrameRange(Start, End); }
            set { Start = value.Start; End = value.End; }
        }
        public List<GEventStyle> getStyles()
        {
            return styles;
        }
        public GEventStyle AddStyle(Type t)
        {
            GEventStyle evt = Activator.CreateInstance(t) as GEventStyle;
            styles.Add(evt);
            return evt;
        }
        public string typeName
        {
            get
            {
                 return this.GetType().Name;
            }
        }
        private GEventAttribute mAttr;
        public GEventAttribute Attr
        {
            get
            {
                if(mAttr == null)
                {
                    GTimelineFactory.GetAttr(this);
                }
                return mAttr;
            }
        }
    }
    public abstract class GEvent : GObject
	{
        public GTimeline root { get; private set; }
        public object mData { get; private set; }
        public bool triggerOnSkip { get { return true; } }

		/// @brief Range of the event.
        public GEventStyle mStyle { get; private set; }
        public GSceneObject ower{ get; private set; }

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
        public GEvent mParent { get; private set; }
        public bool IsRoot { get { return mParent == null; } }
        /// @brief Get the tracks inside this timeline
        protected List<GEvent> _events = new List<GEvent>();
        public List<GEvent> GetEvents() { return _events; }
        public static GEvent Create(GTimeline root, GEventStyle data, GEvent parent)
        {
            GEvent evt = GTimelineFactory.GetEvent(data);
            if (root == null)
            {
                if(evt is GTimeline)
                {
                    root = evt as GTimeline;
                }
                else
                    return null;
            }
            evt.mParent = parent;
            evt.root = root;
            evt.mStyle = data;
            evt.frameRange = data.range;
            for (int i = 0; i < data.styles.Count; i++)
            {
                GEvent child = GEvent.Create(root, data.styles[i], evt);
                child.SetId(i);
                evt._events.Add(child);
            }
            return evt;
        }
        public GEvent Add<T>(T data) where T : GEventStyle
        {
            if (mStyle == null || data == null)
                return null;
            mStyle.styles.Add(data);
            GEvent evt = GEvent.Create(this.root, data, this);
            int id = _events.Count;
            _events.Add(evt);
            evt.SetId(id);
            evt.Init();
            return evt;
        }
        public GEvent Get(int index)
        {
            return _events[index];
        }
        /// @brief Returns event on position \e index, they are ordered left to right.
        public void Remove(GEvent evt)
        {
            if (this.mStyle == null || evt == null)
                return;
            if (_events.Remove(evt))
            {
                this.mStyle.styles.Remove(evt.mStyle);
                evt.Destroy();
                UpdateEventIds();
            }
        }
        public void Rebuild()
        {
            UpdateEventIds();
        }
        private void UpdateEventIds()
        {
            for (int i = 0; i != _events.Count; ++i)
                _events[i].SetId(i);
        }

        #region virtual Function
        protected virtual void OnInit() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnTrigger(int framesSinceTrigger, float timeSinceTrigger) { }
        protected virtual void OnUpdateEvent(int framesSinceTrigger, float timeSinceTrigger){}
        protected virtual void OnFinish() { }
        protected virtual void OnStop() { }
        protected virtual void OnResume() { }
        protected virtual void OnPause() { }
        #endregion
        public void Init()
        {
            _hasTriggered = false;
            _hasFinished = false;
            mTimeSinceTrigger = 0f;
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Init();
            }
            OnInit();
        }
        public void Destroy()
        {
            if (mStyle == null)
                return;
            this.mData = null;
            this.mStyle = null;
            this.root = null;
            this.ower = null;
            for (int i = 0; i != _events.Count; ++i)
                _events[i].Destroy();
            _events.Clear();
            OnDestroy();
            GTimelineFactory.ReleaseEvent(this);
        }

		public void Pause()
		{
#if UNITY_EDITOR
			PreEvent();
#endif
            for (int i = 0; i != _events.Count; ++i)
            {
                if (_events[i].HasTriggered && !_events[i].HasFinished)
                    _events[i].Pause();
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
            for (int i = 0; i != _events.Count; ++i)
            {
                if (_events[i].HasTriggered && !_events[i].HasFinished)
                    _events[i].Resume();
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
            for (int i = _events.Count - 1; i >= 0; --i)
            {
                if (_events[i].HasTriggered)
                    _events[i].Stop();
            }
			OnStop();

#if UNITY_EDITOR
			PostEvent();
#endif
		}
        public void UpdateEvent( int framesSinceTrigger, float timeSinceTrigger )
		{
            mTimeSinceTrigger = timeSinceTrigger;
#if UNITY_EDITOR
			PreEvent();
#endif
			if( !_hasTriggered )
			{
				Trigger( framesSinceTrigger, timeSinceTrigger );
			}
            UpdateChilds(framesSinceTrigger, timeSinceTrigger);
			OnUpdateEvent( framesSinceTrigger,  timeSinceTrigger );
			if( framesSinceTrigger == Length )
			{
				Finish();
			}
#if UNITY_EDITOR
			PostEvent();
#endif
		}
        public virtual void UpdateChilds(int frame, float time)
        {
            int limit = _events.Count;
            int increment = 1;

            if (!root.IsPlayingForward)
            {
                limit = -1;
                increment = -1;
            }
            for (int i = 0; i != limit; i += increment)
            {
                if (frame < _events[i].Start)
                {
                }
                else if (frame >= _events[i].Start && frame <= _events[i].End)
                {
                    _events[i].UpdateEvent(frame - _events[i].Start, time - _events[i].StartTime);
                }
                else //if( frame > _events[_currentEvent].End ) // is it finished
                {
                    if (!_events[i].HasFinished && (_events[i].HasTriggered || _events[i].triggerOnSkip))
                    {
                        _events[i].UpdateEvent(_events[i].Length, _events[i].LengthTime);
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
        public void UpdateEventEditor( int framesSinceTrigger, float timeSinceTrigger )
		{
            mTimeSinceTrigger = timeSinceTrigger;
			PreEvent();

			if( !_hasTriggered )
				Trigger( framesSinceTrigger, timeSinceTrigger );

			OnUpdateEventEditor( framesSinceTrigger, timeSinceTrigger );
            UpdateChildsEditor(framesSinceTrigger, timeSinceTrigger);
			if( framesSinceTrigger == Length )
				Finish();

			PostEvent();
		}
        public virtual void UpdateChildsEditor(int frame, float time)
        {
            int limit = _events.Count;

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
                if (frame < _events[i].Start)
                {
                    if (_events[i].HasTriggered)
                        _events[i].Stop();

                }
                else if (frame >= _events[i].Start && frame <= _events[i].End)
                {
                    if (_events[i].HasFinished)
                        _events[i].Stop();

                    _events[i].UpdateEventEditor(frame - _events[i].Start, time - _events[i].StartTime);
                }
                else //if( currentFrame > _events[i].End )
                {
                    if (!_events[i].HasFinished)
                        _events[i].UpdateEventEditor(_events[i].Length, _events[i].LengthTime);

                }
#if FLUX_PROFILER
				Profiler.EndSample();
#endif
            }
        }
        protected virtual void OnUpdateEventEditor( int framesSinceTrigger, float timeSinceTrigger )
		{
			OnUpdateEvent( framesSinceTrigger, timeSinceTrigger );
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
			return id == root.GetEvents().Count-1;
		}

        public FrameRange frameRange { get; private set; }
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
            return _events.Count == 0;
        }
		/// @brief Does the Event collides the \e e?
		public bool Collides( GEvent e )
		{
            return frameRange.Collides(e.frameRange);
		}

		/// @brief Returns the biggest frame range this event can have
		public FrameRange GetMaxFrameRange()
		{
			FrameRange range = new FrameRange(0, 0);

			if( id == 0 )
			{
				range.Start = 0;
			}
			else
			{
                range.Start = root.GetEvents()[id - 1].End;
			}

            if (id == root.GetEvents().Count - 1) // last one?
			{
                range.End = root.Length;
			}
			else
			{
                range.End = root.GetEvents()[id + 1].Start;
			}

			return range;
		}

		/// @brief Compares events based on their start frame, basically used to order them.
		/// @param e1 Event
		/// @param e2 Event
		public static int Compare( GEvent e1, GEvent e2 )
		{
			return e1.Start.CompareTo( e2.Start );
		}
	}
}