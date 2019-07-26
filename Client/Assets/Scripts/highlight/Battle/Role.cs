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
        AutoFight,
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
        public Dictionary<StateType, StateMachineAction> stateMachineDic = new Dictionary<StateType, StateMachineAction>();
        public StateMachineAction mainStateMachine { get { return GetState(StateType.RoleState); } }
        public StateMachineAction GetState(StateType t)
        {
            StateMachineAction machine = null;
            stateMachineDic.TryGetValue(t, out machine);
            if (machine == null)
                Debug.LogError("machine == null:" + t.ToString());
            return machine;
        }
        public RoleState state { get
            {
                if (mainStateMachine == null)
                    return RoleState.Clear;
                return (RoleState)mainStateMachine.curValue;
            }
        }
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
        //private VInt3 _lastEuler = VInt3.forward;
        public VInt3 location { get { return this._location; } }
        public Vector3 position { get { return (Vector3)location; } }
        public VInt3 forward { get { return this._forward;  } }
        public void SetStateMachine(StateType type, StateMachineAction action)
        {
            stateMachineDic[type] = action;
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
        public void PlayClip(string name,bool loop = false, float speed = 1f)
        {
            animator.speed = speed;
            float off = 0f;
            if (loop && control.curClip == name)
            {
                float len = animator.GetCurrentAnimatorStateInfo(0).length;
                off = App.render_time % len;
              //  return;
            }
            float dur = speed > 0f ? FixedTransitionDuration / speed : FixedTransitionDuration;
            animator.CrossFadeInFixedTime(name, dur, -1, off);
            control.curClip = name;
        }
        public void SetClipSpeed(float speed = 1f)
        {
            animator.speed = speed;
        }
        public Transform getLocator(string name)
        {
            return control.Get(name);
        }
        public void SetPos(Vector3 pos,bool force)
        {
            if (force)
            {
                _lastlocation = this._location;
                transform.position = pos;
                lastInterValue = 1f;
            }
            else
            {
                _lastlocation = this._location;
                lastInterValue = 0f;
            }
            this._location = (VInt3)pos;
        }
        public void SetQuaternion(Quaternion q)
        {
            this.SetForward(q * Vector3.forward);
        }
        public void SetForward(Vector3 euler)
        {
            this._forward = (VInt3)euler;
           // if (force)
          //  {
                transform.forward = euler;
           //     _lastEuler = this._euler;
           // }
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
            PlayEvent();
            if (ai != null)
                ai.UpdateFrame(delta);
            if(attrs != null)
                attrs.UpdateFrame(delta);
            if (skills != null)
                skills.UpdateFrame(delta);
            //if (buffs != null)
            //    buffs.UpdateFrame(delta);
        }
        public virtual void UpdateRender(float interpolation)
        {
            if (lastInterValue >= 1f)
                return;
            if (this._lastlocation != this._location)
            {
            if (lastInterValue > interpolation)
                interpolation = 1f;
                transform.position = Vector3.Lerp((Vector3)this._lastlocation, (Vector3)this._location, interpolation);
                if (interpolation >= 1f)
                    this._lastlocation = this._location;
                lastInterValue = interpolation;
            }
            //if (this._lastEuler != this._euler)
            //{
            //    transform.forward = Vector3.Lerp((Vector3)this._lastEuler, (Vector3)this._euler, interpolation);
            //    if (interpolation >= 1f)
            //        this._lastEuler = this._euler;
            //}
        }
        public void PlayEvent()
        {
            if (this.type != RoleType.Player)
                return;
            ProfilerTest.BeginSample("PlayEvent");
            if (!Events.Contains(this.onlyId))
            {
                if(this.control != null && this.control.curClip == "run")
                    this.PlayClip("wait_1",true);
            }
            else
            {
                RoleEvent evts = Events.Get(this);
                if (evts.isMove)
                {
                    this.PlayClip("run", true);
                    this.SetPos(evts.pos, false);
                }
                if (evts.isDir)
                {
                    this.SetForward(evts.dir);
                }
                if (evts.skillId > 0)
                {
                    //   Debug.Log("CreatSkill:" + evts.skillId);
                    Skill skill = this.skills.Creat(evts.skillId);
                    if (evts.isSkillPos)
                        skill.timeline.target.setPosition(evts.skillPos);
                    skill.timeline.Play(0);
                }
            }
            ProfilerTest.EndSample();
        }
        public bool CanDestroy
        {
            get
            {
                if (this.state != RoleState.Destroy)
                    return false;
                bool b = true;
                if(skills != null && skills.Count > 0)
                {
                    for(int i=0;i<skills.Count;i++)
                    {
                        if(!skills[i].DeadDestroy)
                        {
                            b = false;
                            break;
                        }
                    }
                }
                return b;
            }
        }
        public virtual void Clear()
        {
            SetOnlyId(-1);
            if (skills != null)
                skills.Release();
            //if (buffs != null)
            //    buffs.Release();
            if (attrs != null)
                attrs.Release();
            if (ai != null)
                ai.Destroy();
            stateMachineDic.Clear();
            evtObs.Clear();
            attrs = null;
            skills = null;
            //buffs = null;
            ai = null;
            control = null;
            items = null;
        }
        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (ai != null)
            {
                ai.OnDrawGizmos();
            }
            if (skills != null)
                skills.OnDrawGizmos();
        }
        public List<Timeline> GetAllTimeline()
        {
            List<Timeline> tls = new List<Timeline>();
            if(this.ai != null)
                tls.Add(this.ai);
            if(this.skills != null)
            {
                for(int i=0;i<this.skills.Count;i++)
                { 
                    if (skills[i].timeline != null)
                        tls.Add(skills[i].timeline);
                }
            }
            return tls;
        }
    }
}