using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.timeline
{
   // [Time("Timeline", typeof(Timeline))]
    public class TimelineStyle : TimeStyle
    {
        public int FrameRate = DEFAULT_FRAMES_PER_SECOND;
        public const int DEFAULT_FRAMES_PER_SECOND = 60;
        public const int DEFAULT_LENGTH = 2;
        public float LengthTime { get { return (float)Length / FrameRate; } }
        public static TimelineStyle CreatDefault(string name)
        {
            TimelineStyle creatNew = new TimelineStyle();
            creatNew.name = name;
            creatNew.Range = new FrameRange(0, TimelineStyle.DEFAULT_FRAMES_PER_SECOND * TimelineStyle.DEFAULT_LENGTH);
            return creatNew;
        }
        public static ObjectPool<Timeline> linePool = new ObjectPool<Timeline>();
        public Timeline Creat()
        {
            Timeline root = linePool.Get();
            TimeObject.Create(root, this, null);
            return root;
        }
        public override void release(TimeObject obj)
        {
            Timeline tl = obj as Timeline;
            linePool.Release(tl);
        }
    }
    public class Timeline : TimeObject
    {
        public TimelineStyle lStyle { get { return timeStyle as TimelineStyle; } }
        public Target target = new Target();
        public SceneObject owner = null;
        public float FrameRate
        {
            get
            {
                return lStyle.FrameRate;
            }
        }
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
        public void SetCurrentTime(float time)
        {
            SetCurrentFrame(RoundToInt(time * lStyle.FrameRate));
        }
        public int GetCurrentFrame()
        {
            return _currentFrame;
        }
        public void Play(float curTime, float startTime = 0f)
        {
            Play(curTime, RoundToInt(startTime * lStyle.FrameRate));
        }
        public void Play(float curTime, int startFrame=0)
        {
            if (!_isInit || _isPlaying)
                return;
            _isPlayingForward = true;
            if (!IsStopped)
                Resume();
            _isPlaying = true;
            _lastUpdateTime = curTime;
            SetCurrentFrame(startFrame);
        }
        protected override void OnInit()
        {
            _isInit = true;
            base.OnInit();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            owner = null;
            target.Clear();
        }
        protected override void OnStop()
        {
            _isInit = false;
            if (IsStopped)
                return;
            _isPlaying = false;
            _isPlayingForward = true;
            _currentFrame = -1;
            base.OnStop();
        }
        protected override void OnPause()
        {
            if (!_isPlaying)
                return;
            _isPlaying = false;
            base.OnPause();
        }
        protected override void OnResume()
        {
            if (_isPlaying)
                return;
            _isPlaying = true;
            base.OnResume();
        }
        public void Update(float time)
        {
            if (!_isPlaying)
                return;
            float delta = time - _lastUpdateTime;
            float timePerFrame = 1/FrameRate;
            if (delta >= timePerFrame)
            {
                int numFrames = RoundToInt(delta * FrameRate);
                SetCurrentFrame(_currentFrame + numFrames);
                _lastUpdateTime += timePerFrame * numFrames;
            }
        }
        /// @brief Sets current frame.
        /// @param frame Frame.
        /// @sa Length, GetCurrentFrame
        public void SetCurrentFrame(int frame)
        {
            _currentFrame = Clamp(frame, 0, lStyle.Length);

            _isPlayingForward = _currentFrame >= frame;

            UpdateFrame(_currentFrame);

            if (_currentFrame == lStyle.Length)
            {
                Stop();
            }
        }
        public void SetCurrentFrameEditor(int frame)
        {
#if UNITY_EDITOR
            _isPlayingForward = frame >= _currentFrame;
            _currentFrame = Clamp(frame, 0, lStyle.Length);
#endif
        }
        int RoundToInt(float f)
        {
            return (int)Math.Floor(f + 0.5f);
        }
        int Clamp(int c,int a,int b)
        {
            if (c < a)
                return a;
            if (c > b)
                return b;
            return a;
        }
    }
}