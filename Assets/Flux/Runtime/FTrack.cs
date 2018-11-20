using UnityEngine;
using System;
using System.Collections.Generic;

namespace Flux
{
	/**
	 * @brief FTrack holds events of a specific type. You cannot have a track with multiple types of events.
	 * You can only have 2 events on the same frame per track, i.e. overlapping start and end.
	 * @sa FSequence, FTimeline, FEvent
	 */
	public class FTrack : FObject
	{
		// timeline it belongs to
		[SerializeField]
		[HideInInspector]
		private FTimeline _timeline;

		// string of the type of events it holds
		[SerializeField]
		[HideInInspector]
		private string _evtTypeStr;

		// event type, which unfortunately doesn't serialize
        private Type _evtType;

		// events it holds
		[SerializeField]
		[HideInInspector]
		private List<FEvent> _events = new List<FEvent>();
		/// @brief List of events it holds
		public List<FEvent> GetEvents() { return _events; }

		// keep track of the current event we're updating
		private int _currentEvent = 0;

		/**
		 * @brief Creates a FTrack.
		 * @param T type of event this track will hold
		 */
		public static FTrack Create<T>() where T : FEvent
		{
			Type evtType = typeof(T);

			string evtName = evtType.Name;
			GameObject trackGO = new GameObject( evtName );

			Type trackType = typeof(FTrack);
			object[] customAttributes = evtType.GetCustomAttributes(typeof(FEventAttribute), false);
			if( customAttributes.Length > 0 )
			{
				trackType = ((FEventAttribute)customAttributes[0]).trackType;
			}

			FTrack track = (FTrack)trackGO.AddComponent(trackType);
            //track._evtType = evtType;
            //track._evtTypeStr = evtType.ToString();
            track.SetEventType( evtType );

//			CEvent evt = CEvent.Create<T>( range );
//
//			track.Add( evt );
//
//			track._timeline = timeline;
//			track.SetId( id );

			return track;
		}

		// sets the timeline it belongs to, only to be called by FTimeline to avoid errors
		internal void SetTimeline( FTimeline timeline )
		{
			if( _timeline == timeline )
				return;
			_timeline = timeline;
			if( _timeline )
			{
				transform.parent = _timeline.transform;
			}
			else
			{
				transform.parent = null;
			}
		}

		public override FSequence Sequence { get { return _timeline.Sequence; } }

		public override Transform Owner { get { return _timeline.Owner; } }

		public override void Init()
		{
			_currentEvent = Sequence.IsPlayingForward ? 0 : _events.Count - 1;

			for( int i = 0; i != _events.Count; ++i )
				_events[i].Init();
		}

		/// @brief Pauses the timeline
		public virtual void Pause()
		{
			for( int i = 0; i != _events.Count; ++i )
			{
				if( _events[i].HasTriggered && !_events[i].HasFinished )
					_events[i].Pause();
			}
		}

		/// @brief Resumes the timeline
		public virtual void Resume()
		{
			for( int i = 0; i != _events.Count; ++i )
			{
				if( _events[i].HasTriggered && !_events[i].HasFinished )
					_events[i].Resume();
			}
		}

		public override void Stop()
		{
			for( int i = _events.Count-1; i >= 0; --i )
			{
				if( _events[i].HasTriggered )
					_events[i].Stop();
			}

			_currentEvent = Sequence.IsPlayingForward ? 0 : _events.Count - 1;
		}

		/// @brief Returns true if the track has no events
		public bool IsEmpty()
		{
			return _events.Count == 0;
		}

		/// @brief Returns to which timeline this track belongs
		public FTimeline GetTimeline()
		{
			return _timeline;
		}

		/// @brief Returns which type of event this track holds
		public Type GetEventType()
		{
            if( _evtType == null )
            {
                _evtType = Type.GetType( _evtTypeStr );
            }
			return _evtType;
		}

		/// @brief Sets the type of event this track holds
		/// @param evtType Has to inherit from FEvent
        private void SetEventType( Type evtType )
        {
			if( !evtType.IsSubclassOf( typeof(FEvent) ) )
				throw new ArgumentException( evtType.ToString() + " does not inherit from FEvent");
            _evtType = evtType;
            _evtTypeStr = evtType.ToString();
        }

		/// @brief Returns event on position \e index, they are ordered left to right.
		public FEvent GetEvent( int index )
		{
			return _events[index];
		}

		/// @brief Returns events at a specific frame.
		public int GetEventsAt( int t, ref FEvent[] evtBuffer )
		{
			int index = 0;

			for( int i = 0; i != _events.Count; ++i )
			{
				if( _events[i].Start <= t && _events[i].End >= t )
					evtBuffer[index++] = _events[i];
				else if( _events[i].Start > t ) // since they are ordered, no point in continuing
					break;
			}

			return index;
		}

		/// @brief Returns events at a given frame
		/// @param [in] t Frame
		/// @param [out] first First event, if there's 2 events it will be the one ending on that frame
		/// @param [out] second Second event, if there's 2 events it will be the one starting on that frame
		/// @return How many events are at frame \e t
		public int GetEventsAt( int t, out FEvent first, out FEvent second )
		{
			int index = 0;

			first = null;
			second = null;

			for( int i = 0; i != _events.Count; ++i )
			{
				if( _events[i].Start <= t && _events[i].End >= t )
				{
					if( index == 0 )
						first = _events[i];
					else
						second = _events[i];
					++index;
				}
				else if( _events[i].Start > t ) // since they are ordered, no point in continuing
					break;
			}
			
			return index;
		}

		public FEvent GetEventBefore( int t )
		{
			for( int i = 0; i != _events.Count; ++i )
			{
				if( _events[i].Start >= t )
				{
					if( i > 0 )
					{
						return _events[i-1];
					}
					return null;
				}
			}

			return null;
		}

		public FEvent GetEventAfter( int t )
		{
			for( int i = _events.Count-1; i >= 0; --i )
			{
				if( _events[i].End <= t )
				{
					if( i < _events.Count-1 )
					{
						return _events[i+1];
					}
					return null;
				}
			}

			return _events.Count > 0 ? _events[0] : null;
		}

		public bool CanAdd( FEvent evt )
		{
			foreach( FEvent e in _events )
			{
				// abort search, events are ordered!
				if( e.Start > evt.End )
				{
					break;
				}

				if( evt.Collides( e ) )
				{
					return false;
				}
			}
			return true;
		}

		public bool CanAdd( FEvent evt, FrameRange newRange )
		{
			for( int i = 0; i != _events.Count; ++i )
			{
				if( _events[i].Start > newRange.End )
					break;

				if( _events[i] == evt )
					continue;

				if( _events[i].FrameRange.Collides( newRange ) )
					return false;
			}

			return true;
		}

		public bool CanAddAt( int t )
		{
			foreach( FEvent e in _events )
			{
				if( e.Start < t + 1 && e.End > t )
				{
					return false;
				}
			}

			return true;
		}

        public bool CanAddAt( int t, out int maxLength )
        {
            maxLength = 0;

            if( t < 0 )
            {
                return false;
            }

            for( int i = 0; i != _events.Count; ++i )
            {
                if( _events[i].Start > t )
                {
                    maxLength = _events[i].Start - t;
                    return true;
                }
                else if( _events[i].Start <= t && _events[i].End > t )
                {
                    return false;
                }
            }

			if( t >= Sequence.Length - 1 )
            {
                return false;
            }

			maxLength = Sequence.Length - t;
            return true;
        }

        public void Add( FEvent evt )
		{
			evt.SetTrack( this );

			for( int i = 0, limit = _events.Count; i != limit; ++i )
			{
				if( _events[i].Start > evt.End )
				{
					_events.Insert(i, evt);
					UpdateEventIds();
					return;
				}
			}

			// didn't find a spot, add at the end
			evt.SetId( _events.Count );
			_events.Add( evt );
			if( !Sequence.IsStopped )
				evt.Init();
		}

		public void Remove( FEvent evt )
		{
			_events.Remove( evt );
			evt.SetTrack( null );
			UpdateEventIds();
		}

		public virtual void UpdateEvents( int frame, float time )
		{
			int limit = _events.Count;
			int increment = 1;
			
			if( !Sequence.IsPlayingForward )
			{
				limit = -1;
				increment = -1;
			}


			for( int i = _currentEvent; i != limit; i += increment )
			{
				if( frame < _events[i].Start )
				{
					if( Sequence.IsPlayingForward )
						break;
					else
						_currentEvent = Mathf.Clamp( i - 1, 0, _events.Count-1 );
				}
				else if( frame >= _events[i].Start && frame <= _events[i].End )
				{
					_events[i].UpdateEvent( frame - _events[i].Start, time-_events[i].StartTime );
				}
				else //if( frame > _events[_currentEvent].End ) // is it finished
				{
					if( !_events[i].HasFinished && (_events[i].HasTriggered || _events[i].TriggerOnSkip) )
					{
						_events[i].UpdateEvent( _events[i].Length, _events[i].LengthTime );
					}

					if( Sequence.IsPlayingForward )
						_currentEvent = Mathf.Clamp( i + 1, 0, _events.Count-1);
					else
						break;
				}
			}
		}

		public virtual void UpdateEventsEditor( int frame, float time )
		{
			int limit = _events.Count;

			if( limit == 0 )
				return;

			int increment = 1;

			if( !Sequence.IsPlayingForward )
			{
				limit = -1;
				increment = -1;
			}

			for( int i = _currentEvent; i != limit; i += increment )
			{
#if FLUX_PROFILER
				Profiler.BeginSample("Event: " + i + " " + _events[i].name );
#endif
				if( frame < _events[i].Start )
				{
					if( _events[i].HasTriggered )
						_events[i].Stop();

					if( Sequence.IsPlayingForward )
						break;
					else
						_currentEvent = Mathf.Clamp( i - 1, 0, _events.Count-1);
				}
				else if( frame >= _events[i].Start && frame <= _events[i].End )
				{
					if( _events[i].HasFinished )
						_events[i].Stop();

					_events[i].UpdateEventEditor( frame - _events[i].Start, time - _events[i].StartTime );
				}
				else //if( currentFrame > _events[i].End )
				{
					if( !_events[i].HasFinished )
						_events[i].UpdateEventEditor( _events[i].Length, _events[i].LengthTime );

					if( Sequence.IsPlayingForward )
						_currentEvent = Mathf.Clamp( i + 1, 0, _events.Count-1 );
					else
						break;
				}
#if FLUX_PROFILER
				Profiler.EndSample();
#endif
			}
		}

		public virtual void CreatePreview()
		{

		}

		public virtual void ClearPreview()
		{

		}

		public virtual bool CanCreatePreview()
		{
			return true;
		}

		public virtual bool CanTogglePreview()
		{
			return false;
		}

		private bool _hasPreview = false;

		private bool _isPreviewing = false;

		public bool HasPreview { get { return _hasPreview; } protected set { _hasPreview = value; } }

		public bool IsPreviewing
		{
			get { return _isPreviewing; }
			set
			{
				_isPreviewing = value;

				if( _isPreviewing )
					CreatePreview();
				else
					ClearPreview();
			}
		}

		public void Rebuild()
		{
			Transform t = transform;
			_events.Clear();

			for( int i = 0; i != t.childCount; ++i )
			{
				FEvent evt = t.GetChild(i).GetComponent<FEvent>();
				if( evt )
				{
					evt.SetTrack( this );
					_events.Add( evt );
				}
			}

			UpdateEventIds();
		}

		public void UpdateEventIds()
		{
			_events.Sort( delegate( FEvent c1, FEvent c2 ) { return c1.FrameRange.Start.CompareTo( c2.FrameRange.Start ); } );

			for(int i = 0, limit = _events.Count; i != limit; ++i )
			{
				_events[i].SetId( i );
			}
		}

        public FrameRange GetValidRange( FEvent evt )
        {
            int index = 0;
            for( ; index < _events.Count; ++index )
            {
                if( _events[index] == evt )
                {
                    break;
                }
            }

            FrameRange range = new FrameRange( 0, Sequence.Length );

            if( index > 0 )
            {
                range.Start = _events[index - 1].End;
            }
            if( index < _events.Count - 1 )
            {
                range.End = _events[index + 1].Start;
            }

			if( range.Length > evt.GetMaxLength() )
				range.Length = evt.GetMaxLength();

            return range;
        }
	}
}
