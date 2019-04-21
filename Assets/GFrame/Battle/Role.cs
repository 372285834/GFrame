using highlight.tl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    public enum RoleType
    {
        Player,
        Monster,
        Npc,
        Item,
        Build,
    }
    public enum RoleCamp
    {
        Red,
        Blue,
        Middle,
    }
    public enum RoleState
    {
        Clear = 0,
        Idle,
        Move,
        Hit,
        Attack,
        AutoFight,
        Dead,
    }
    public partial class Role : Object
    {
        public RoleType type;
        public RoleCamp camp;//阵营
        private RoleState _state = RoleState.Clear;
        public RoleState state { get { return _state; } }
        private ObserverV<Role> obs_state = new ObserverV<Role>();
        public bool isClear { get { return _state == RoleState.Clear; } }
        public object data;
        public RoleControl entity;
        public Timeline ai;
        public RoleAttrs attrs;
        public Skills skills;
        public Buffs buffs;
        public Equips quips;
        public void AddObs_State(AcHandler<Role> ac)
        {
            obs_state.AddObserver(ac);
        }
        public void RemoveObs_State(AcHandler<Role> ac)
        {
            obs_state.RemoveObserver(ac);
        }
        public void Switch(RoleState _state)
        {
            if (_state != this.state)
            {
                this._state = _state;
                obs_state.Change(this);
            }
        }
        public Transform transform { get { return entity.transform; } }
        public Animator animator { get { return entity.mAnimator; } }
        //public AnimationBox aniBox;
        public void Init(RoleControl _entity)
        {
            entity = _entity;
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
            this.Switch(RoleState.Clear);
            if (skills != null)
                skills.Release();
            if (buffs != null)
                buffs.Release();
            if (attrs != null)
                attrs.Release();
            if (ai != null)
                ai.Destroy();

            obs_state.Clear();
            attrs = null;
            skills = null;
            buffs = null;
            ai = null;
            entity = null;
        }
    }
}