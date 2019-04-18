using UnityEngine;
using System.Collections;
using System;
namespace highlight
{
    /// <summary>
    /// 动画暂停效果
    /// </summary>
    public class AnimationBox : MonoBehaviour
    {
        public Animation mAnimation;
        public AnimationClip curClip
        {
            get
            {
                return mAnimation.GetClip(mCurName);
            }
        }
        public string mCurName;
        public string[] clips = null;
        float mOldSpeed = 0f;
        private bool mIsPause = false;
        void Awake()
        {
            if (mAnimation == null)
                mAnimation = GetComponent<Animation>();
            //if (mAnimation == null)
            //    return;
            //if(clips != null && clips.Length > 0)
            //{
            //     for(int i=0;i<clips.Length;i++)
            //     {
            //         AnimationClip clip = ResourceManager.GetClip(clips[i]);
            //         mAnimation.AddClip(clip, clip.name);
            //     }
            //}
            //clips = null;
            //if (!string.IsNullOrEmpty(mCurName))
            //    mAnimation.clip = curClip;
        }
        //void Start()
        //{
        //    if (mAnimation != null && mAnimation.playAutomatically && !string.IsNullOrEmpty(mCurName))
        //        mAnimation.Play(mCurName, PlayMode.StopAll);
        //}
        //void OnEnable()
        //{
        //    if (mAnimation != null)
        //        this.mAnimation.enabled = true;
        //}
        //void OnDisable()
        //{
        //    if (mAnimation != null)
        //        this.mAnimation.enabled = false;
        //}
        public static AnimationBox GetAnimationBox(GameObject go)
        {
            return go.AddComp<AnimationBox>();
        }
        public float PlayAndDefault(string animationName, int mode, float speed,string defAction)
        {
            PlayQueued(defAction);
            return Play(animationName, mode, speed);
        }
        public void ContinuePlay(string animationName)
        {
            if(mAnimation != null)
            {
                if(!mAnimation.isPlaying)
                {
                    mAnimation.Play(animationName);
                }
            }
        }
        public float Play(string animationName)
        {
            return Play(animationName, 0, 1f);
        }
        public float Play(string animationName, int mode, float speed)
        {
            if (mAnimation == null)
                return 0f;
            if (mIsPause)
                return 0f;
            if (IsPlayDead())
                return 0f;
            if(!mAnimation.enabled)
			    mAnimation.enabled = true;
            AnimationClip clip = mAnimation.GetClip(animationName);
            if (clip == null)
                return 0f;
            //curClip = clip;
            //if (mAnimation.IsPlaying(animationName))
            //    return false;

            mAnimation.clip = clip;
            //mAnimation.playAutomatically = false;
            mCurName = animationName;
            if (speed != 1)
            {
                mAnimation[animationName].speed = speed;
            }
            if (mode == 0|| animationName == "dead")
            {
                if (mAnimation.isPlaying)
                    mAnimation.Stop();
                mAnimation.Play(animationName);
            }
            else if (mode == 1)
            {
                // bool b = mAnimation.Play(animationName, PlayMode.StopAll);
                // return b;
                mAnimation.CrossFade(animationName, 0.1f);
            }
            return clip.length;
        }
        public void PlayQueued(string animationName)
        {
            AnimationClip clip = mAnimation.GetClip(animationName);
            if (clip != null)
                mAnimation.CrossFadeQueued(animationName,0.5f);
        }
        public bool IsPlayDead()
        {
            return mAnimation != null && mAnimation.IsPlaying("dead");
        }
        public void Stop(string animationName)
        {
            if (mAnimation && mAnimation[animationName])
            {
                mAnimation.Stop(animationName);
            }
        }
        public void Stop()
        {
            //mAnimation.Stop();
            if (!string.IsNullOrEmpty(this.mCurName))
            {
                Stop(mCurName);
            }
        }
        public void Pause()
        {
            if (mIsPause)
                return;
            if (mAnimation == null)
                return;
            mIsPause = true;
            AnimationState state = mAnimation[mCurName];
            if (state != null)
            {
                mOldSpeed = state.speed;
                state.speed = 0f;                
            }
        }
        public AnimationClip GetClip(string name)
        {
            return mAnimation.GetClip(name);
        }
        public void Resume()
        {
            if (!mIsPause)
                return;
            if (mAnimation == null)
                return;
            mIsPause = false;
            AnimationState state = mAnimation[mCurName];
            if (state != null)
            {
                state.speed = 1;               
            }
        }

    }
}