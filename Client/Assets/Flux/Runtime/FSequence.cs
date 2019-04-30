using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * @brief Flux is top namespace for everything pertaining to the runtime.
 */
namespace Flux
{
	/**
	 * @brief FSequence is the main runtime class. It holds all the timelines and controls their behaviour.
	 * @sa FTimeline, FTrack, FEvent
	 */
	public class FSequence : MonoBehaviour
	{
		public const int DEFAULT_FRAMES_PER_SECOND = 60;
		public const int DEFAULT_LENGTH = 10;

		public static FSequence CreateSequence()
		{
			return CreateSequence( new GameObject("FSequence") );
		}

		public static FSequence CreateSequence( GameObject gameObject )
		{
			FSequence sequence = gameObject.AddComponent<FSequence>();

			Transform timelineContainer = new GameObject( "Timeline Container" ).transform;
			timelineContainer.hideFlags = HideFlags.HideInHierarchy;
			timelineContainer.parent = sequence.transform;

			sequence._timelineContainer = timelineContainer;

			return sequence;
		}

		[SerializeField]
		[HideInInspector]
		/// @brief Transform inside the Sequence, which is hidden by default to prevent errors
		/// and contains all the timelines inside
		private Transform _timelineContainer = null;
		public Transform TimelineContainer { get { return _timelineContainer; } }

		/// @brief Should the sequence start playing on the Start() function?
		[SerializeField]
		private bool _playOnStart = false;

		[SerializeField]
		private bool _loop = false;
		public bool Loop { get { return _loop; } set { _loop = value; } }

		/// @brief Should it update on FixedUpdate? If false, it will update on Update.
		[SerializeField]
		private AnimatorUpdateMode _updateMode = AnimatorUpdateMode.Normal;
		public AnimatorUpdateMode UpdateMode { get { return _updateMode; } set { _updateMode = value; } }

		/// @brief The length of this sequence in frames.
		[SerializeField]
		private int _length = DEFAULT_LENGTH * DEFAULT_FRAMES_PER_SECOND;
		/// @brief Length of this sequence in frames.
		public int Length { get { return _length; } set { _length = value; } }

		/// @brief Returns the length of this sequence in seconds.
		/// @Warning This value isn't cached internally, so avoid calling it all the time.
		private float LengthTime { get { return (float)_length * _inverseFrameRate; } }


		[SerializeField]
		private List<FTimeline> _timelines = new List<FTimeline>();
		/// @brief List of CTimelines. They are in the order they will be evaluated.
		public List<FTimeline> GetTimelines() { return _timelines; }

        [SerializeField]
		[HideInInspector]
		private int _frameRate = DEFAULT_FRAMES_PER_SECOND; // frame rate

		/** @brief Frame Rate of this sequence.
		 * @Warning Although we allow you to change this value at runtime, you should be careful in how you use it. 
		 * Changing this value \b will not \b rescale the sequence. Rescaling the sequence should only be done in editor, 
		 * not at runtime.
		 * @sa CSequenceInspector.Rescale(CSequence, int)
		 * @sa InverseFrameRate
		 */
		public int FrameRate { get { return _frameRate; } set { _frameRate = value; _inverseFrameRate = 1f / _frameRate; } }

        [SerializeField]
		[HideInInspector]
        private float _inverseFrameRate = 1f / DEFAULT_FRAMES_PER_SECOND;

		/// @brief Returns 1 / FrameRate. This value is cached and set automatically when FrameRate is called.
		/// @sa FrameRate
		public float InverseFrameRate { get { return _inverseFrameRate; } }

		// has it been initialized?
		private bool _isInit = false;
		/// @brief Is the sequence initialized?
		public bool IsInit { get { return _isInit; } }

		// Is the sequence playing?
		private bool _isPlaying = false;
		/// @brief Is the sequence playing?
		public bool IsPlaying { get { return _isPlaying; } }

		// Is the sequence playing forward?
		private bool _isPlayingForward = true;
		/// @brief Is the sequence moving forward?
		public bool IsPlayingForward { get { return _isPlayingForward; } }

		// time we last updated
		private float _lastUpdateTime = 0;

		/// @brief Adds new timeline at the end of the list.
		/// @param timeline New timeline.
		public void Add( FTimeline timeline )
		{
			int id = _timelines.Count;

			_timelines.Add( timeline );
			timeline.SetId( id );
			
			timeline.SetSequence( this );
		}

		/// @brief Removes timeline and updates their ids.
		/// @param timeline CTimeline to remove.
		/// @note After calling this function, the ids of the timelines after this
		/// one in the list will have an id smaller by 1.
		public void Remove( FTimeline timeline )
		{
			_timelines.Remove( timeline );
			timeline.SetSequence( null );

			UpdateTimelineIds();
		}

		/// @brief Removes timeline with id.
		/// @oaram id Id of the CTimeline to remove.
		/// @note After calling this function, the ids of the timelines after this
		/// one in the list will have an id smaller by 1.
		/// @warning Does not check if id is valid (i.e. between -1 & GetTimelines().Count)
		public void Remove( int id )
		{
			FTimeline timeline = _timelines[id];
			_timelines.RemoveAt( id );
			timeline.SetSequence( null );

			UpdateTimelineIds();
		}

		// Current frame.
		private int _currentFrame = -1;

		/// @brief Sets current frame.
		/// @param frame Frame.
		/// @sa Length, GetCurrentFrame
		public void SetCurrentFrame( int frame )
		{
			_currentFrame = Mathf.Clamp( frame, 0, Length );

			_isPlayingForward = _currentFrame >= frame;

            float currentTime = _currentFrame * InverseFrameRate;

			for( int i = 0; i != _timelines.Count; ++i )
			{
				_timelines[i].UpdateTracks( _currentFrame, currentTime );
			}

			if( _currentFrame == Length )
			{
				if( _loop ) 
				{
					Stop();
					Play();
				}
				else 
				{
					Pause();
				}
			}
		}

		/// @brief Sets the current frame manually, i.e. to be used in Editor mode, _shouldn't_ be used at runtime
		/// @param frame Frame.
		/// @sa SetCurrentFrame
		public void SetCurrentFrameEditor( int frame )
		{
#if UNITY_EDITOR
			_isPlayingForward = frame >= _currentFrame;
			_currentFrame = Mathf.Clamp( frame, 0, Length );
#endif
		}


		/// @brief Sets the current frame based on the time passed.
		/// @param time Time in seconds, this will set current frame with time x FrameRate.
		public void SetCurrentTime( float time )
		{
			SetCurrentFrame( Mathf.RoundToInt(time * _frameRate) );
		}

		/// @brief Returns the current frame.
		/// @sa SetCurrentFrame, GetCurrentFrame
		public int GetCurrentFrame()
		{
			return _currentFrame;
		}

		/// @brief Initializes the sequence.
		/// This is called at the start of the sequence, it is meant for the
		/// user to setup all the cached variables.
		/// @note If you want to avoid frame drops, call this function before
		/// calling Play.
		public void Init()
		{
#if FLUX_DEBUG
			Debug.Log("Init");
#endif

#if FLUX_PROFILE
			Profiler.BeginSample( "FSequence.Init()" );
#endif
			_isInit = true;

			for( int i = 0; i != _timelines.Count; ++i )
				_timelines[i].Init();

#if FLUX_PROFILE
			Profiler.EndSample();
#endif
		}

		/// @brief Starts playing on a specific frame.
		/// @sa Init
		public void Play( int startFrame )
		{
			if( _isPlaying )
				return;


			_isPlayingForward = true;

			if( !_isInit )
				Init();

			if( !IsStopped )
				Resume();

			_isPlaying = true;

			switch( _updateMode )
			{
			case AnimatorUpdateMode.Normal:
				_lastUpdateTime = Time.time;
				break;
			case AnimatorUpdateMode.AnimatePhysics:
				_lastUpdateTime = Time.fixedTime;
				break;
			case AnimatorUpdateMode.UnscaledTime:
				_lastUpdateTime = Time.unscaledTime;
				break;
			default:
				Debug.LogError("Unsupported Update Mode");
				_lastUpdateTime = Time.time;
				break;
			}

			SetCurrentFrame( startFrame );
		}

		/// @brief Starts playing at time.
		/// @param startTime What time to start playing from. Will simply set the current frame to startTime x FrameRate.
		/// @sa Init
		public void Play( float startTime )
		{
			Play( Mathf.RoundToInt( startTime * _frameRate ) );
		}

		/// @brief Starts playing from current frame.
		/// @sa Init, Stop, Pause
		public void Play()
		{
			Play( 0 );
		}

		/// @brief Stops sequence.
		public void Stop()
		{
			Stop( false );
		}

		public void Stop( bool reset )
		{
			if( reset )
				_isInit = false;


			if( IsStopped )
				return;

#if FLUX_DEBUG
			Debug.Log ("Stop");
#endif
			_isPlaying = false;
			_isPlayingForward = true;

			for( int i = 0; i != _timelines.Count; ++i )
				_timelines[i].Stop();

			_currentFrame = -1;
		}

		/**
		 * @brief Pauses sequence.
		 * @sa FEvent.OnPause, FEvent.OnResume
		 */
		public void Pause()
		{
			if( !_isPlaying )
				return;
			_isPlaying = false;

			for( int i = 0; i != _timelines.Count; ++i )
				_timelines[i].Pause();
		}

		/**
		 * @brief Resumes a sequence that is paused.
		 * Doesn't work if the sequence is stopped.
		 * @sa Play, Stop, Pause
		 */
		public void Resume()
		{
			if( _isPlaying )
				return;

			_isPlaying = true;

			for( int i = 0; i != _timelines.Count; ++i )
				_timelines[i].Resume();
		}


		/// @brief Is the sequence paused?
		public bool IsPaused { get { return !_isPlaying && _currentFrame >= 0; } }

		/// @brief Is the sequence stopped?
		public bool IsStopped { get { return _currentFrame < 0; } }

		/// @brief Does the sequence have no events?
		public bool IsEmpty()
		{
			foreach( FTimeline timeline in _timelines )
			{
				if( !timeline.IsEmpty() )
					return false;
			}

			return true;
		}

		protected virtual void Start()
		{
			if( _playOnStart )
				Play();
		}

		// Updates the sequence when update mode is NOT AnimatePhysics
		protected virtual void Update()
		{
			if( _updateMode == AnimatorUpdateMode.AnimatePhysics || !_isPlaying )
			{
				return;
			}

			InternalUpdate( _updateMode == AnimatorUpdateMode.Normal ? Time.time : Time.unscaledTime );
		}

		// Updates the sequence when update mode is AnimatePhysics
		protected virtual void FixedUpdate()
		{
			if( _updateMode != AnimatorUpdateMode.AnimatePhysics || !_isPlaying )
			{
				return;
			}

			InternalUpdate( Time.fixedTime );
		}

		// Internal update function, i.e. does the actual update of the sequence
		protected virtual void InternalUpdate( float time )
		{
			float delta = time - _lastUpdateTime;
			float timePerFrame = InverseFrameRate;
			if( delta >= timePerFrame )
			{
				int numFrames = Mathf.RoundToInt(delta / timePerFrame);
				SetCurrentFrame( _currentFrame + numFrames );
				_lastUpdateTime = time - (delta - (timePerFrame * numFrames));
			}
		}

		/// @brief Rebuilds the sequence. This is to be called whenever timelines,
		/// tracks, or events are added / removed from the sequence.
		/// @note You should only call this in editor mode, avoid calling it at runtime.
		public void Rebuild()
		{
#if FLUX_DEBUG
			Debug.Log("Rebuilding");
#endif
			Transform t = TimelineContainer;
			_timelines.Clear();

			for( int i = 0; i != t.childCount; ++i )
			{
				FTimeline timeline = t.GetChild(i).GetComponent<FTimeline>();

				if( timeline )
				{
					_timelines.Add( timeline );
					timeline.SetSequence( this );
					timeline.Rebuild();
				}
			}

			UpdateTimelineIds();
		}

		// Updates the ids of the timelines
		private void UpdateTimelineIds()
		{
			for( int i = 0; i != _timelines.Count; ++i )
			{
				_timelines[i].SetId( i );
			}
		}

#if FLUX_TRIAL

		private Rect _watermarkLabelRect = new Rect(0,0,0,0);
		private GUIStyle _watermarkLabelStyle;
		private float _watermarkEndTime;
		private float _watermarkAlpha;

		private void OnGUI()
		{
			if( !IsPlaying )
				return;

			float watermarkDuration = 3f;

			GUIContent watermark = new GUIContent("..::FLUX TRIAL::..");

			if( _watermarkLabelRect.width == 0 )
			{
				_watermarkLabelStyle = new GUIStyle(GUI.skin.label);
				_watermarkLabelStyle.fontSize = 24;

				Vector2 size = _watermarkLabelStyle.CalcSize( watermark );

				_watermarkLabelRect.x = Random.Range(0, Screen.width-size.x);
				_watermarkLabelRect.y = Random.Range(0, 2);
				if( _watermarkLabelRect.y == 1 )
					_watermarkLabelRect.y = Screen.height-size.y;
				_watermarkLabelRect.width = size.x;
				_watermarkLabelRect.height = size.y;

				_watermarkEndTime = Time.time + watermarkDuration;
				_watermarkAlpha = 1.6f;
			}
			GUI.color = new Color(1f, 1f, 1f, _watermarkAlpha + 0.4f );
			GUI.Label( _watermarkLabelRect, watermark, _watermarkLabelStyle );
			if( Time.time < _watermarkEndTime )
			{
				_watermarkAlpha -= Time.deltaTime / watermarkDuration;
				if( _watermarkAlpha < 0 )
					_watermarkAlpha = 0;
			}
		}
#endif
	}
}

