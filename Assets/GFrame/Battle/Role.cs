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
        public bool isClear { get { return _state == RoleState.Clear; } }
        public object data;
        public RoleControl entity;
        public Timeline ai;
        //private RoleAttrs _attrs;
        //public RoleAttrs attrs { get { if (_attrs == null) _attrs = new RoleAttrs(this);return _attrs; } }
        public Skills skills;
        public Buffs buffs;
        private Dictionary<AttrType, IAttrValue> dic = new Dictionary<AttrType, IAttrValue>();
        private ObserverV<IAttrValue> obs_value = new ObserverV<IAttrValue>();
        private ObserverV<RoleState> obs_state = new ObserverV<RoleState>();

        private readonly static ObjectPool<IntAttr> intPool = new ObjectPool<IntAttr>();
        private readonly static ObjectPool<BoolAttr> boolPool = new ObjectPool<BoolAttr>();

        public void AddObs_Attr(AcHandler<IAttrValue> ac)
        {
            obs_value.AddObserver(ac);
        }
        public void RemoveObs_Attr(AcHandler<IAttrValue> ac)
        {
            obs_value.RemoveObserver(ac);
        }
        protected void Change(IAttrValue t)
        {
            obs_value.Change(t);
        }
        public void AddObs_State(AcHandler<RoleState> ac)
        {
            obs_state.AddObserver(ac);
        }
        public void RemoveObs_State(AcHandler<RoleState> ac)
        {
            obs_state.RemoveObserver(ac);
        }
        public void Switch(RoleState _state)
        {
            if (_state != this.state)
            {
                this._state = _state;
                obs_state.Change(_state);
            }
        }
        public IntAttr GetIntAttr(AttrType t, bool add = false)
        {
            IAttrValue v = null;
            dic.TryGetValue(t, out v);
            if (v == null && add)
            {
                IntAttr iv = intPool.Get();
                iv.type = t;
                v = iv;
                dic[t] = v;
            }
            return (IntAttr)v;
        }
        public void AddIntAttr(AttrType t, IntValue v)
        {
            IntAttr list = GetIntAttr(t, true);
            list.value += v;
            list.GetValue();
            this.Change(list);
        }
        public void AddIntAttr(AttrType t,IIntAttrValue v)
        {
            IntAttr list = GetIntAttr(t, true);
            list.AddValue(v);
            this.Change(list);
        }
        public bool RemoveIntAttr(AttrType t, IIntAttrValue v)
        {
            IntAttr list = GetIntAttr(t, true);
            bool b = list.RemoveValue(v);
            this.Change(list);
            return b;
        }
        public BoolAttr GetBoolAttr(AttrType t, bool add = false)
        {
            IAttrValue v = null;
            dic.TryGetValue(t, out v);
            if (v == null && add)
            {
                BoolAttr iv = boolPool.Get();
                iv.type = t;
                v = iv;
                dic[t] = v;
            }
            return (BoolAttr)v;
        }
        public void AddBoolAttr(AttrType t, IBoolAttrValue v)
        {
            BoolAttr list = GetBoolAttr(t, true);
            list.AddValue(v);
            this.Change(list);
        }
        public bool RemoveBoolAttr(AttrType t, IBoolAttrValue v)
        {
            BoolAttr list = GetBoolAttr(t, true);
            bool b = list.RemoveValue(v);
            this.Change(list);
            return b;
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
            if (ai != null)
                ai.Destroy();

            foreach (var v in dic.Values)
            {
                v.ClearValue();
                if (v is IntAttr)
                    intPool.Release((IntAttr)v);
                else if (v is IntAttr)
                    boolPool.Release((BoolAttr)v);
            }
            dic.Clear();
            obs_value.Clear();
            obs_state.Clear();

            skills = null;
            buffs = null;
            ai = null;
            entity = null;
        }
    }
}