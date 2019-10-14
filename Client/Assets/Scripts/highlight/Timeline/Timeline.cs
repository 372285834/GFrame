using System;
using System.Collections;
using System.Collections.Generic;

namespace highlight.tl
{
   // [Time("Timeline", typeof(Timeline))]
    public class TimelineStyle : TimeStyle
    {
        public bool loop = false;
        public bool forever = false;
        [Newtonsoft.Json.JsonIgnore]
        public int FrameRate { get { return DEFAULT_FRAMES_PER_SECOND; } }
        public const int DEFAULT_FRAMES_PER_SECOND = 60;
        public const int DEFAULT_LENGTH = 2;
        [Newtonsoft.Json.JsonIgnore]
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
        public bool forever = false;
        public float FrameRate { get { return TimelineStyle.DEFAULT_FRAMES_PER_SECOND; } }
        public Target target = new Target();
        public Role owner = null;
       // public Buff buff = null;
        public Skill skill = null;
        public Dictionary<string, TimeObject> nodeDic = new Dictionary<string, TimeObject>();
        public Dictionary<string, TimeAction> actionDic = new Dictionary<string, TimeAction>();
        public Dictionary<string, object> globalDic = new Dictionary<string, object>();
        public Observer<string, object> globalObs = new Observer<string, object>();
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
            Play(curTime, RoundToInt(startTime * FrameRate));
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
            _currentFrame = startFrame;
            UpdateFrame(0);
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
        public T FindAction<T>(string name) where T: TimeAction
        {
            return FindAction(name) as T;
        }
        public T GetGlobal<T>(string k)
        {
            object data = null;
            this.globalDic.TryGetValue(k, out data);
            if (data != null)
                return (T)data;
            return default(T);
        }
        public bool SetGlobalValue(string k,object v)
        {
            if(globalDic.ContainsKey(k))
            {
                globalDic[k] = v;
                globalObs.Change(k, v);
                return true;
            }
            return false;
        }
        public override void Init()
        {
            if (_isInit)
                return;
            _isInit = true;
            this.forever = this.lStyle.forever;
           // this.FrameRate = this.lStyle.FrameRate;
            List<ComponentData> datas = this.ComponentList;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] is IGlobalData)
                {
                    IGlobalData data = (datas[i] as IGlobalData);
                    this.globalDic[data.key] = data.GetValue();
                }
            }
            base.Init();
            //this.Reset();
        }
        public override void Destroy()
        {
            if (!IsStopped)
                Stop();
            _isInit = false;
            base.Destroy();
            owner = null;
         //   buff = null;
            skill = null;
            target.Clear();
            nodeDic.Clear();
            actionDic.Clear();
            globalDic.Clear();
            globalObs.Clear();
        }
        public override void Stop(bool reset = false)
        {
            if (IsStopped)
                return;
            _isPlaying = false;
            _isPlayingForward = true;
            _currentFrame = -1;
            base.Stop(reset);
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
                UpdateFrame(numFrames);
                _lastUpdateTime += timePerFrame * numFrames;
            }
        }
        public void SetCurrentTime(float delta)
        {
            UpdateFrame(RoundToInt(delta * FrameRate));
        }
        /// @brief Sets current frame.
        /// @param frame Frame.
        /// @sa Length, GetCurrentFrame
        public override void UpdateFrame(int delta)
        {
            int frame = delta + _currentFrame;
            if (!_isPlaying)
                return;
            ProfilerTest.BeginSample(this.name);
            if(this.forever)
            {
                _currentFrame = 0;
            }
            else
            {
                _currentFrame = Clamp(frame, 0, this.Length);
                _isPlayingForward = _currentFrame >= frame;
            }

            _UpdateFrame(_currentFrame);

            if (!this.forever && _currentFrame >= this.Length)
            {
                Stop(this.lStyle.loop);
                if (this.lStyle.loop)
                {
                    _isPlayingForward = true;
                    _isPlaying = true;
                    _currentFrame = 0;
                    if(frame > this.Length)
                    {
                        UpdateFrame(frame - this.Length);
                    }
                }
            }
            ProfilerTest.EndSample();
        }
//#if UNITY_EDITOR
//        public void SetCurrentFrameEditor(int frame)
//        {
//            _isPlayingForward = frame >= _currentFrame;
//            _currentFrame = Clamp(frame, 0, this.Length);
//            UpdateEditor(_currentFrame);
//        }
//#endif
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