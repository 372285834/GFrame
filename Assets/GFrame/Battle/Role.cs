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
        Bullet,
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
        private Observer<RoleState,Role> obs_state = new Observer<RoleState,Role>();
        public bool isClear { get { return _state == RoleState.Clear; } }
        public object data;
        public RoleControl control;
        public Timeline ai;
        public RoleAttrs attrs;
        public Skills skills;
        public Buffs buffs;
        public RoleItems items;

        public bool isInterpolation = true;
        private VInt3 _location = VInt3.zero;
        private VInt3 _lastlocation = VInt3.zero;
        private VInt3 _euler = VInt3.forward;
        private VInt3 _lastEuler = VInt3.forward;
        public VInt3 location { get { return this._location; } }
        public Vector3 position { get { return (Vector3)location; } }
        public VInt3 euler { get { return this._euler;  } }
        public void AddObs_State(AcHandler<RoleState, Role> ac)
        {
            obs_state.AddObserver(ac);
        }
        public void RemoveObs_State(AcHandler<RoleState, Role> ac)
        {
            obs_state.RemoveObserver(ac);
        }
        public void Switch(RoleState _state)
        {
            if (_state != this.state)
            {
                this._state = _state;
                obs_state.Change(_state,this);
            }
        }
        public Transform transform { get { return control.transform; } }
        public Animator animator { get { return control.mAnimator; } }
        //public AnimationBox aniBox;
        public void Init(RoleControl _entity)
        {
            control = _entity;
        }
        public void PlayClip(string name,float duration,float speed)
        {
            animator.speed = speed;
            animator.CrossFadeInFixedTime(name, duration);
        }
        public Transform getLocator(string name)
        {
            return control.Get(name);
        }
        public void SetPos(Vector3 pos,bool force)
        {
            this._location = (VInt3)pos;
            if(force)
            {
                _lastlocation = this._location;
                transform.position = pos;
            }
        }
        public void SetRotate(Vector3 euler, bool force)
        {
            this._euler = (VInt3)euler;
            if (force)
            {
                transform.eulerAngles = euler;
                _lastEuler = this._euler;
            }
        }
        public void SetParent(Transform t,bool reset = true)
        {
            transform.SetParent(t);
            if(reset)
            {
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.localRotation = Quaternion.identity;
            }
        }
        public virtual void UpdateFrame(int delta)
        {
            if (ai != null)
                ai.UpdateFrame(delta);
            if (skills != null)
                skills.UpdateFrame(delta);
            if (buffs != null)
                buffs.UpdateFrame(delta);
        }
        public virtual void UpdateRender(float interpolation)
        {
            if (!isInterpolation)
                return;
            if(this._lastlocation != this._location)
            {
                transform.position = Vector3.Lerp((Vector3)this._lastlocation, (Vector3)this._location, interpolation);
                if (interpolation >= 1f)
                    this._lastlocation = this._location;
            }
            if (this._lastEuler != this._euler)
            {
                transform.forward = Vector3.Lerp((Vector3)this._lastEuler, (Vector3)this._euler, interpolation);
                if (interpolation >= 1f)
                    this._lastEuler = this._euler;
            }
            
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
            control = null;
            items = null;
        }
    }
}