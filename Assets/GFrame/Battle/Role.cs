using highlight.tl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    public class Role : Object
    {
        public RoleControl entity;
        public Timeline ai;
        public RoleAttrs attrs;
        public Skills skills;
        public Buffs buffs;
        public bool isClear = false;
        public Transform transform { get { return entity.transform; } }
        public Animator animator { get { return entity.mAnimator; } }
        //public AnimationBox aniBox;
        public void Init(RoleControl _entity)
        {
            entity = _entity;
            isClear = false;
        }
        public void PlayClip(string name,float duration,float speed)
        {
            animator.speed = speed;
            animator.CrossFadeInFixedTime(name, duration);
        }
        public Vector3 getPosition()
        {
            return transform.position;
        }
        public Transform getLocator(string name)
        {
            return entity.Get(name);
        }
        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }
        public void SetLocalPos(Vector3 pos)
        {
            transform.localPosition = pos;
        }
        public void SetParent(Transform t)
        {
            transform.SetParent(t);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
        public virtual void UpdateFrame(int frame)
        {
            if (ai != null)
                ai.UpdateFrame(frame);
            if(attrs != null)
                attrs.UpdateFrame(frame);
            if (skills != null)
                skills.UpdateFrame(frame);
            if (buffs != null)
                buffs.UpdateFrame(frame);
        }
        public virtual void UpdateRender()
        {

        }
        public virtual void Clear()
        {
            if (skills != null)
                skills.Release();
            if (buffs != null)
                buffs.Release();
            if (attrs != null)
                attrs.Release();
            if (ai != null)
                ai.Destroy();
            skills = null;
            buffs = null;
            ai = null;
            isClear = true;
        }
    }
}