using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
   // [Time("Timeline", typeof(Timeline))]
    public class TimelineStyle : TimeStyle
    {
        public bool loop = false;
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
            root.DestroyOnStop = true;
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
        public Buff buff = null;
        public Skill skill = null;
        public Dictionary<string, TimeObject> nodeDic = new Dictionary<string, TimeObject>();
        public Dictionary<string, TimeAction> actionDic = new Dictionary<string, TimeAction>();
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
        public bool IsPaused { get { return _isInit && !_isPlaying && _currentFrame >= 0; } }

        /// @brief Is the sequence stopped?
        public bool IsStopped { get { return _currentFrame < 0; } }

        public bool DestroyOnStop = true;
        public int GetCurrentFrame()
        {
            return _currentFrame;
        }
        public void Play(float curTime, float startTime = 0f)
        {
            Play(curTime, RoundToInt(startTime * lStyle.FrameRate));
        }
        public void Play(float curTime, int startFrame)
        {
            if (!_isInit || _isPlaying)
                return;
            _isPlayingForward = true;
            if (!IsStopped)
                Resume();
            _isPlaying = true;
            _lastUpdateTime = curTime;
            UpdateFrame(startFrame);
        }
        public TimeObject FindObj(string name)
        {
            TimeObject obj = null;
            nodeDic.TryGetValue(name, out obj);
            return obj;
        }
        public TimeAction FindAction(string key)
        {
            TimeAction action = null;
            actionDic.TryGetValue(key, out action);
            return action;
        }
        public override void Init()
        {
            if (_isInit)
                return;
            _isInit = true;
            base.Init();
        }
        public override void Destroy()
        {
            if (!IsStopped)
                Stop();
            _isInit = false;
            base.Destroy();
            owner = null;
            buff = null;
            skill = null;
            target.Clear();
            nodeDic.Clear();
            actionDic.Clear();
        }
        public override void Stop()
        {
            if (IsStopped)
                return;
            _isPlaying = false;
            _isPlayingForward = true;
            _currentFrame = -1;
            base.Stop();
        }
        public override void Pause()
        {
            if (!_isPlaying)
                return;
            _isPlaying = false;
            base.Pause();
        }
        public override void Resume()
        {
            if (_isPlaying)
                return;
            _isPlaying = true;
            base.Resume();
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
                UpdateFrame(_currentFrame + numFrames);
                _lastUpdateTime += timePerFrame * numFrames;
            }
        }
        public void SetCurrentTime(float time)
        {
            UpdateFrame(RoundToInt(time * lStyle.FrameRate));
        }
        /// @brief Sets current frame.
        /// @param frame Frame.
        /// @sa Length, GetCurrentFrame
        public void UpdateFrame(int frame)
        {
            if (!_isPlaying)
                return;
            _currentFrame = Clamp(frame, 0, this.Length);

            _isPlayingForward = _currentFrame >= frame;

            _UpdateFrame(_currentFrame);

            if (_currentFrame == this.Length)
            {
                if(this.lStyle.loop)
                {
                    _currentFrame = 0;
                    base.Init();
                    return;
                }
                Stop();
            }
        }
#if UNITY_EDITOR
        public void SetCurrentFrameEditor(int frame)
        {
            _isPlayingForward = frame >= _currentFrame;
            _currentFrame = Clamp(frame, 0, this.Length);
            UpdateEditor(_currentFrame);
        }
#endif
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
            return c;
        }
    }
}