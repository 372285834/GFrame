using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace highlight
{
    [System.Serializable]
    public class Curve3D
    {
        public int id;
        public AnimationCurve ProgressCurve;
        public AnimationCurve CrossCurve;
        public float CrossValue;
        public Vector2 CrossClamp = Vector2.zero;
        public AnimationCurve TangentCurve;
        public float TangentValue;
        public Vector2 TangentClamp = Vector2.zero;
        public void Active()
        {
            if (CrossClamp.x < CrossClamp.y)
            {
                CrossValue = Random.Range(CrossClamp.x, CrossClamp.y);
            }
            if (TangentClamp.x < TangentClamp.y)
            {
                TangentValue = Random.Range(TangentClamp.x, TangentClamp.y);
            }
        }
        public Vector3 Evaluate(Vector3 start, Vector3 end, float time)
        {
            float pro = time;
            if (ProgressCurve != null && ProgressCurve.length > 0)
                pro = ProgressCurve.Evaluate(time);
            Vector3 next = Vector3.Lerp(start, end, pro);
            return Evaluate(start, end, time, next);
        }
        public Vector3 Evaluate(Vector3 start, Vector3 end, float time, Vector3 next)
        {
            //if (time >= 1f)
            //    return end;
            //Vector3 cross = Vector3.Cross(start, end).normalized;
            //cross.y = Mathf.Abs(cross.y);
            if (Mathf.Abs(CrossValue) > 0.0001f)
            {
                next.y += CrossCurve.Evaluate(time) * CrossValue;
            }
            if (Mathf.Abs(TangentValue) > 0.0001f)
            {
                Vector3 tangent = Vector3.Cross(end - start, Vector3.up).normalized;
                next += TangentCurve.Evaluate(time) * TangentValue * tangent;
            }
            return next;
        }
    }
    [System.Serializable]
    public class Motion
    {
        [System.Serializable]
        public class MotionData
        {
            private Motion m_motion;
            public Motion motion { get { return m_motion; } set { m_motion = value; } }
            public bool useCurve = false;
            public Curve3D curve3D;
            [SerializeField]
            private Vector3 m_pos;
            public float delay;
            public float rotateTime = 0f;
            public float scale = 1f;
            public Vector3 pos {
                get { return m_motion == null ? m_pos : m_pos + m_motion.offPos; }
                set { m_pos = value; }
            }
            public MotionData() { }
            public MotionData(Vector3 pos, float d, Motion m)
            {
                Init(pos, d, m);
            }
            public void Init(Vector3 pos, float d, Motion m)
            {
                m_pos = pos;
                this.m_motion = m;
                delay = d;
            }
        }
        public AnimationCurve progressCurve;
        public WrapMode mMode = WrapMode.Once;
        public int Length { get { return link.Count; } }
        [SerializeField]
        private int m_curIdx = 0;
        private int curIdx { 
            get
            {
                return m_curIdx;
            }
            set
            {
                m_curIdx = value;
                nextIdx = CreatNextIndx();
            }
        }
        public float Progress { get { return curProgress; } }
        private float curProgress;
        public float curTime;
        [Range(0.01f,100f)]
        public float speed = 1f;
        public float curTotalTime { get { return curDis / speed; } }
        private float curDis = 1f;
        public float mDelay = 0f;
        public Vector3 offPos = Vector3.zero;
        public Vector3 nextPos;
        public bool autoRotate = false;
        public List<MotionData> link = new List<MotionData>();
        public Vector3 mCurPos { get { return curPos; } }
        private Vector3 curPos = Vector3.zero;
        public bool isReverse = false;
        public AcHandler acArrive;
        public AcHandler startArrive;
        public void Add(Vector3 pos, float delay)
        {
            MotionData md = new MotionData(pos, delay, this);
            Add(md);
        }
        public void Add(MotionData md)
        {
            link.Add(md);
        }
        public void Remove(MotionData md)
        {
            link.Remove(md);
            if (curData != null && curData == md)
                goNext();
        }
        public void Start()
        {
            for (int i = 0; i < link.Count; i++)
            {
                link[i].motion = this;
            }
            Move(curIdx);
        }
        public void Move(int idx = 0)
        {
            curProgress = 0f;
            if (idx >= link.Count)
                idx = 0;
            this.curIdx = idx;
            if (nextData == null)
                return;
            nextPos = curData.pos;
            curDis = Mathf.Abs(Vector3.Distance(curData.pos, nextData.pos));
            mDelay = curData.delay;
        }
        public bool Valid()
        {
            return this.link.Count < 2 || curData == null || nextData == null || curDis <= 0.0001f || speed <= 0.0001f;
        }
        public bool Update(float deltaTime)
        {
            if (Valid())
                return false;
            bool b = false;
            if (mDelay > 0 && deltaTime>0f)
            {
                mDelay -= deltaTime;
                if (mDelay <= 0 && startArrive != null)
                    startArrive();
            }
            else
            {
                //float deltaPro = deltaTime / curTotalTime;
                curTime += deltaTime * curData.scale;
                curProgress = curTime / curTotalTime;
                curProgress = progressCurve.Evaluate(curProgress);
                curPos = nextPos;
                nextPos = GetCurvePos(curData,nextData, curProgress);// Vector3.Lerp(curData.pos, nextData.pos, curProgress);
                if (curProgress >= 1f)
                {
                    goNext();
                }
                b = true;
            }
            return b;
        }

        public Vector3 GetCurvePos(MotionData cur,MotionData next, float time,bool useReverse = true)
        {
            useReverse &= isReverse;
            bool useCurve = useReverse ? next.useCurve : cur.useCurve;
            Curve3D curve3D = useReverse ? next.curve3D : cur.curve3D;
            if (useCurve && curve3D != null)
                if (useReverse)
                    return curve3D.Evaluate(next.pos, cur.pos, 1-time);
                else
                    return curve3D.Evaluate(cur.pos, next.pos, time);
            else
                return Vector3.Lerp(cur.pos, next.pos, time);
        }
        private void goNext()
        {
            curTime = 0f;
            curIdx = nextIdx;
            if (nextIdx >= 0)
            {
                Move(curIdx);
            }else
            {
                Clear();
            }
            if (acArrive != null)
                acArrive();

        }
        public Vector3 GetPos(int idx)
        {
            if (idx >= 0 && idx < link.Count)
                return link[idx].pos;
            return Vector3.zero;
        }
        public void SetPos(int idx,Vector3 pos)
        {
            if (idx >= 0 && idx < link.Count)
                link[idx].pos = pos;
        }
        public MotionData curData
        {
            get
            {
                if (curIdx >= link.Count)
                    return null;
                return link[curIdx];
            }
        }
        public MotionData nextData
        {
            get
            {
                if (nextIdx >= 0 && nextIdx < link.Count)
                    return link[nextIdx];
                return null;
            }
        }
        public int nextIdx { get; private set; }
        private int CreatNextIndx()
        {
            if (link.Count == 0)
                return -1;
            int idx = this.curIdx;
            switch (mMode)
            {
                case WrapMode.Loop:
                    idx++;
                    if (idx > this.Length - 1)
                    {
                        this.curIdx = 0;
                        idx = 1;
                    }
                    break;
                case WrapMode.PingPong:
                    if (curIdx >= this.Length - 1)
                        isReverse = true;
                    else if (curIdx <= 0)
                        isReverse = false;
                    int v = isReverse ? -1 : 1;
                    idx += v;
                    break;
                default:
                    if (idx >= this.Length - 1)
                        idx = -1;
                    else
                        idx++;
                    break;
            }
            return idx;
            
        }
        public void Clear()
        {
            if (Application.isPlaying)
                this.link.Clear();
            //this.curProgress = 0f;
            
            isReverse = false;
            curTime = 0f;
            curIdx = 0;
            this.curPos = nextPos;
            //curProgress = 0f;
        }

        public bool SetRotate(Transform trans,bool isLocal = true)
        {
            if (this.autoRotate && this.curData != null)
            {
                if (this.curData.delay <= 0f)
                {
                    Vector3 rotate = nextPos - curPos;
                    if(rotate != Vector3.zero)
                    {
                        if (isLocal)
                            trans.localRotation = Quaternion.LookRotation(rotate);
                        else
                            trans.rotation = Quaternion.LookRotation(rotate);
                        return true;
                    }
                }
            }
            return false;
        }
        public void SetPosition(Transform trans, bool isLocal = true)
        {
            if (isLocal)
                trans.localPosition = this.mCurPos;
            else
                trans.position = this.mCurPos;
        }
    }
}