using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace GP
{
    [GEvent("Timeline", typeof(GTimeline))]
    public class GTimelineStyle : GEventStyle
    {
        public string name;
        public int FrameRate = DEFAULT_FRAMES_PER_SECOND;
        public AnimatorUpdateMode UpdateMode = AnimatorUpdateMode.Normal;
        public const int DEFAULT_FRAMES_PER_SECOND = 60;
        public const int DEFAULT_LENGTH = 2;
        [JsonIgnore]
        public float InverseFrameRate { get { return 1f / FrameRate; } }
        [JsonIgnore]
        public float LengthTime { get { return (float)Length / FrameRate; } }
        public static GTimelineStyle CreatDefault(string name)
        {
            GTimelineStyle creatNew = new GTimelineStyle();
            creatNew.name = name;
            creatNew.range = new FrameRange(0, GTimelineStyle.DEFAULT_FRAMES_PER_SECOND * GTimelineStyle.DEFAULT_LENGTH);
            return creatNew;
        }
    }
    public class GTimeline:GEvent
    {
        public GTimelineStyle lStyle { get { return mStyle as GTimelineStyle; } }
        public List<GSceneObject> targetList = new List<GSceneObject>();
        public Vector3 position;
        public float InverseFrameRate { get { return lStyle.InverseFrameRate; } }
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
        // Current frame.
        private int _currentFrame = -1;
        /// @brief Is the sequence paused?
        public bool IsPaused { get { return !_isPlaying && _currentFrame >= 0; } }

        /// @brief Is the sequence stopped?
        public bool IsStopped { get { return _currentFrame < 0; } }
        public void SetCurrentFrameEditor(int frame)
        {
#if UNITY_EDITOR
            _isPlayingForward = frame >= _currentFrame;
            _currentFrame = Mathf.Clamp(frame, 0, lStyle.Length);
#endif
        }
        public void SetCurrentTime(float time)
        {
            SetCurrentFrame(Mathf.RoundToInt(time * lStyle.FrameRate));
        }
        public int GetCurrentFrame()
        {
            return _currentFrame;
        }
        public void Play(float startTime)
        {
            Play(Mathf.RoundToInt(startTime * lStyle.FrameRate));
        }
        public void Play(int startFrame)
        {
            if (!_isInit || _isPlaying)
                return;
            _isPlayingForward = true;
            if (!IsStopped)
                Resume();
            _isPlaying = true;

            switch (lStyle.UpdateMode)
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

            SetCurrentFrame(startFrame);
        }
        protected override void OnInit()
        {
            _isInit = true;
        }
        protected override void OnDestroy()
        {
            this.targetList.Clear();
        }
        protected override void OnStop()
        {
             _isInit = false;
            if (IsStopped)
                return;
            _isPlaying = false;
            _isPlayingForward = true;
            _currentFrame = -1;
        }
        protected override void OnPause()
        {
            if (!_isPlaying)
                return;
            _isPlaying = false;
        }
        protected override void OnResume()
        {
            if (_isPlaying)
                return;
            _isPlaying = true;
        }
        public void Update()
        {
            if (lStyle.UpdateMode == AnimatorUpdateMode.AnimatePhysics || !_isPlaying)
            {
                return;
            }
            InternalUpdate(lStyle.UpdateMode == AnimatorUpdateMode.Normal ? Time.time : Time.unscaledTime);
        }
        public void FixedUpdate()
        {
            if (lStyle.UpdateMode != AnimatorUpdateMode.AnimatePhysics || !_isPlaying)
            {
                return;
            }
            InternalUpdate(Time.fixedTime);
        }
        protected virtual void InternalUpdate(float time)
        {
            float delta = time - _lastUpdateTime;
            float timePerFrame = InverseFrameRate;
            if (delta >= timePerFrame)
            {
                int numFrames = Mathf.RoundToInt(delta / timePerFrame);
                SetCurrentFrame(_currentFrame + numFrames);
                _lastUpdateTime = time - (delta - (timePerFrame * numFrames));
            }
        }
        /// @brief Sets current frame.
        /// @param frame Frame.
        /// @sa Length, GetCurrentFrame
        public void SetCurrentFrame(int frame)
        {
            _currentFrame = Mathf.Clamp(frame, 0, lStyle.Length);

            _isPlayingForward = _currentFrame >= frame;

            float currentTime = _currentFrame * InverseFrameRate;
            UpdateEvent(_currentFrame, currentTime);

            if (_currentFrame == lStyle.Length)
            {
                Stop();
            }
        }
    }
}