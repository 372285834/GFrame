using highlight.tl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace highlight
{
    public enum RoleType
    {
        Player=0,
        Monster,
        Npc,
        Item,
        Bullet,
        Build,
    }
    public enum RoleCamp
    {
        Red = 0,
        Blue,
        Middle,
    }
    public enum RoleState
    {
        Clear = 0,
        Idle,
        Move,
        Fight_Idle,
        Fight_Move,
        Hit,
        Attack,
        Skill,
        Dead,
        Destroy,
    }
    public enum RoleObsType
    {
        Hit,//受到攻击
        Attack,//攻击别人
    }
    public partial class Role : Object
    {
        public RoleType type;
        public RoleCamp camp;//阵营
        public bool isClear { get { return this.state >= RoleState.Dead; } }
        public object data;
        public RoleControl control;
        public Timeline ai;
        public RoleAttrs attrs;
        public Skills skills;
        //public Buffs buffs;
        public RoleItems items;

        public float lastInterValue = 0f;
        private VInt3 _location = VInt3.zero;
        private VInt3 _lastlocation = VInt3.zero;
        private VInt3 _forward = VInt3.forward;
        private VInt3 _lastForward = VInt3.forward;
        public VInt3 location { get { return this._location; } }
        public Vector3 position { get { return (Vector3)location; } }
        public VInt3 forward { get { return this._forward;  } }
        public StateMachineAction[] stateMachineDic = new StateMachineAction[2];
        public StateMachineAction mainStateMachine { get { return stateMachineDic[0]; } }
        public StateMachineAction GetState(StateType t)
        {
            return stateMachineDic[(int)t];
        }
        public void Switch(RoleState _state, bool reset = false)
        {
            mainStateMachine.Switch((int)_state, reset);
        }
        public RoleState state
        {
            get
            {
                return mainStateMachine.role_state;
            }
        }
        public void SetStateMachine(StateType type, StateMachineAction action)
        {
            stateMachineDic[(int)type] = action;
        }
        private Observer<RoleObsType, object> evtObs = new Observer<RoleObsType, object>();

        public void AddEvent(AcHandler<RoleObsType, object> ac)
        {
            evtObs.AddObserver(ac);
        }
        public void RemoveEvent(AcHandler<RoleObsType, object> ac)
        {
            evtObs.RemoveObserver(ac);
        }
        protected void ChangeEvent(RoleObsType t)
        {
            evtObs.Change(t, this);
        }

        public Transform transform { get { return control.transform; } }
        public Animator animator { get { return control.mAnimator; } }
        //public AnimationBox aniBox;
        public void SetControl(GameObject go)
        {
            control = RoleControl.Add(go);
            control.id = this.onlyId;
        }
        public void SetControl(RoleControl _entity)
        {
            control = _entity;
            control.id = this.onlyId;
        }
        public static float FixedTransitionDuration = 0.1f;
        public void PlayClip(string name,bool loop = false, float speed = 1f,int length = 1)
        {
            animator.speed = speed;
            float off = 0f;
           // float len = GetClipLength(name);
            if (loop && control.curClip == name)
            {
                off = 0.001f * (App.time % length);
              //  return;
            }
            float dur = speed > 0f ? FixedTransitionDuration / speed : FixedTransitionDuration;
           // ProfilerTest.BeginSample("PlayClip_" + name);
            animator.CrossFadeInFixedTime(name, dur, -1, off);
            control.curClip = name;
            //ProfilerTest.EndSample();
          //  return len;
        }
        public void SetClipSpeed(float speed = 1f)
        {
            animator.speed = speed;
        }
        public float GetClipLength(string name)
        {
            return control.GetClipLength(name);
        }
        public Transform getLocator(string name)
        {
            return control.Get(name);
        }
        public void SetPos(Vector3 pos, bool force)
        {
            SetPosVInt3((VInt3)pos,force);
        }
        public void SetPosVInt3(VInt3 pos,bool force)
        {
            if (force)
            {
                _lastlocation = pos;
                transform.position = (Vector3)pos;
                lastInterValue = 1f;
            }
            else
            {
                _lastlocation = this._location;
                lastInterValue = 0f;
            }
            this._location = pos;
        }
        public void SetQuaternion(Quaternion q, bool force)
        {
            this.SetForward(q * Vector3.forward, force);
        }
        public void SetForward(Vector3 forward, bool force)
        {
         //   if (force)
         //   {
           //     _lastForward = (VInt3)forward;
                transform.forward = forward;
            //    lastInterValue = 1f;
            //}
            //else
            //{
            //    _lastForward = _forward;
            //    lastInterValue = 0f;
            //}
            this._forward = (VInt3)forward;
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
            if (attrs != null)
                attrs.UpdateFrame(delta);
            //if (buffs != null)
            //    buffs.UpdateFrame(delta);
        }
        public virtual void UpdateRender(float interpolation)
        {
            if (lastInterValue >= 1f)
                return;
            if (lastInterValue > interpolation)
                interpolation = 1f;
            lastInterValue = interpolation;
            if (this._lastlocation != this._location)
            {
                transform.position = Vector3.Lerp((Vector3)this._lastlocation, (Vector3)this._location, interpolation);
                if (interpolation >= 1f)
                    this._lastlocation = this._location;
            }
            //if (this._lastForward != this._forward)
            //{
            //    Vector3 dir = SetRotationAction.Slerp((Vector3)this._lastForward, (Vector3)this._forward, interpolation);
            //    transform.forward = dir;// Vector3.Lerp((Vector3)this._lastForward, (Vector3)this._forward, interpolation);
            //    if (interpolation >= 1f)
            //        this._lastForward = this._forward;
            //}
        }
        public bool CanDestroy
        {
            get
            {
                if (this.state != RoleState.Destroy)
                    return false;
                bool b = true;
                //if(skills != null && skills.Count > 0)
                //{
                //    for(int i=0;i<skills.Count;i++)
                //    {
                //        if(!skills[i].DeadDestroy)
                //        {
                //            b = false;
                //            break;
                //        }
                //    }
                //}
                return b;
            }
        }
        public virtual void Destroy()
        {
            SetOnlyId(-1);
            if (skills != null)
                skills.Release();
            //if (buffs != null)
            //    buffs.Release();
            if (attrs != null)
                attrs.Release();
            TimelineFactory.Destroy(ai);
            for (int i = 0; i < stateMachineDic.Length; i++)
                stateMachineDic[i] = null;
            evtObs.Clear();
            attrs = null;
            skills = null;
            //buffs = null;
            ai = null;
            data = null;
            control = null;
            items = null;
        }
        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            if (ai != null)
            {
                UnityEditor.Handles.DrawWireDisc(this.position, Vector3.up, 0.5f);
                ai.OnDrawGizmos();
               // Gizmos.color = Color.blue;
               // Gizmos.DrawLine(this.position, this.position + (Vector3)this.forward);
            }
            if (skills != null)
                skills.OnDrawGizmos();
#endif
        }

    }
}