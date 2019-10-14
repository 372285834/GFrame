using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace highlight
{
    //public enum AttrCalcType
    //{
    //    Add_Base,
    //    Add_Extra,
    //    Add_BasePer,
    //    Add_TotalPer,
    //    Reduce_Base,
    //    Reduce_Extra,
    //    Reduce_BasePer,
    //    Reduce_TotalPer,
    //}
    public interface IAttrValue
    {
        AttrType type{get;set;}
        Observer<RoleAttrs> obs { get; set; }
        void ClearValue();
        bool UpdateValue();
        int Count { get; }
    }
    //public interface IPropAttrValue
    //{
    //    PropValue GetValue();
    //}
    //public interface IBoolAttrValue
    //{
    //    BoolValue GetValue();
    //}
    public class PropAttr : AttrValue<PropValue> {
        public int resultValue;
        public override bool UpdateValue()
        {
            base.result = this.value;
            bool change = false;
            for (int i = this.Count - 1; i >= 0; i--)
            {
                PropValue prop = this[i];
                if(prop.cd.IsComplete)
                {
                    change = true;
                    this.RemoveAt(i);
                   // Debug.Log("Delete Buff2:" + prop.id);
                }
                else
                {
                  //  Debug.Log("Buff2:" + prop.id + ",cd:" + prop.cd.ToString());
                    base.result += prop;
                }
                    
            }
            resultValue = base.result.value;
            return change;
        }
        public override bool RemoveValue(int id)
        {
            bool b = false;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].id == id)
                {
                    b = true;
                    this[i] = this[i].Clear();
                    //Debug.Log("Delete Buff1:" + id + ",cd:" + this[i].cd.ToString());
                }
            }
            if (b)
                this.UpdateValue();
            return b;
        }
    }
    public class BoolAttr : AttrValue<BoolValue>
    {
        public bool resultValue;
        public override bool UpdateValue()
        {
            base.result = this.value;
            bool change = false;
            for (int i = this.Count - 1; i >= 0; i--)
            {
                BoolValue cur = this[i];
                if (cur.cd.IsComplete)
                {
                    change = true;
                    this.RemoveAt(i);
                }
                else
                {
                    if (cur.level > base.result.level)
                        base.result = cur;
                }
            }
            resultValue = base.result.value;
            return change;
        }
        public override bool RemoveValue(int id)
        {
            bool b = false;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].id == id)
                {
                    b = true;
                    this[i] = this[i].Clear();
                  //  Debug.Log("Delete Buff1:" + id + ",cd:" + this[i].cd.ToString());
                }
            }
            if (b)
                this.UpdateValue();
            return b;
        }
    }
    public class AttrValue<T> : List<T>, IAttrValue
    {
        public AttrType type {get;set;}
        public Observer<RoleAttrs> obs { get; set; }
        public T value;
        public T result;
        public virtual bool UpdateValue()
        {
            result = value;
            return false;
        }
        public virtual void AddValue(T v)
        {
            this.Add(v);
            this.UpdateValue();
        }
        public virtual bool RemoveValue(int id)
        {
            return false;
        }
        public virtual void ClearValue()
        {
            if (obs != null)
                obs.Clear();
            this.Clear();
        }
    }
    public struct BoolValue
    {
        public int id;
        public int level;
        public bool value;
        public CDData cd;
        public BoolValue(bool v, int lv = 0)
        {
            id = Id.Global.generateNewId();
            value = v;
            level = lv;
            cd = CDData.Min;
        }
        public BoolValue Clear()
        {
            cd = cd.Clear();
            return this;
        }
        public BoolValue Reset()
        {
            cd = cd.Reset();
            return this;
        }
    }
    public struct PropValue
    {
        public static float ratio = 0.001f;
        public readonly static PropValue zero = new PropValue(0);
        public int id;
        public int baseValue;
        public int extraValue;
        public int basePer;
        public int totalPer;
        public CDData cd;
        public PropValue(int bv)
        {
            id = Id.Global.generateNewId();
            baseValue = bv;
            extraValue = 0;
            basePer = 0;
            totalPer = 0;
            cd = CDData.Min;
        }
        public PropValue(int bv, int ev, int br, int er)
        {
            id = Id.Global.generateNewId();
            baseValue = bv;
            extraValue = ev;
            basePer = br;
            totalPer = er;
            cd = CDData.Min;
        }
        public void SetValue(int bv, int ev, int br, int er)
        {
            baseValue = bv;
            extraValue = ev;
            basePer = br;
            totalPer = er;
        }
        public int value
        {
            get
            {
                int aValue = this.baseValue + this.extraValue;
                float baseR = ratio * this.baseValue * this.basePer;
                float totalR = 1 + ratio * this.totalPer;
                float result = (aValue + baseR) * totalR;
                return (int)Math.Round((double)(result));
            }
        }

        public float showValue
        {
            get { return value * ratio; }
        }
        public static PropValue operator +(PropValue a, PropValue b)
        {
            PropValue c = a;
            c.baseValue += b.baseValue;
            c.basePer += b.basePer;
            c.extraValue += b.extraValue;
            c.totalPer += b.totalPer;
            return c;
        }
        public PropValue Clear()
        {
            cd = cd.Clear();
            return this;
        }
        public PropValue Reset()
        {
            cd = cd.Reset();
            return this;
        }
        //public static IntValue operator -(IntValue a, IntValue b)
        //{
        //    IntValue c = a;
        //    c.baseValue -= b.baseValue;
        //    c.basePer -= b.basePer;
        //    c.extraValue -= b.extraValue;
        //    c.totalPer -= b.totalPer;
        //    return c;
        //}
        //public static IntValue Calculation(AttrCalcType aType, int v, IntValue attr)
        //{
        //    switch (aType)
        //    {
        //        case AttrCalcType.Add_Base:
        //            attr.baseValue += v;
        //            break;
        //        case AttrCalcType.Add_Extra:
        //            attr.extraValue += v;
        //            break;
        //        case AttrCalcType.Add_BasePer:
        //            attr.basePer += v;
        //            break;
        //        case AttrCalcType.Add_TotalPer:
        //            attr.totalPer += v;
        //            break;
        //        case AttrCalcType.Reduce_Base:
        //            attr.baseValue -= v;
        //            break;
        //        case AttrCalcType.Reduce_Extra:
        //            attr.extraValue -= v;
        //            break;
        //        case AttrCalcType.Reduce_BasePer:
        //            attr.basePer -= v;
        //            break;
        //        case AttrCalcType.Reduce_TotalPer:
        //            attr.totalPer -= v;
        //            break;
        //        default:
        //            break;
        //    }
        //    return attr;
        //}
    }
    public enum AttrType
    {
        lv = 1,
        exp,
        move_speed,
        field,
        atk_rang,

        hp = 20,
        max_hp,
        mp,
        max_mp,
        recovery_hp,
        recovery_mp,
        def_phy,
        def_magic,
        toughness,//韧性 减少控制时间
        def_reduce_speed,//减少减速时间
        dodge,//闪避

        cd = 50,
        hit_rate,//命中率
        atk_speed,
        atk_phy,
        atk_magic,
        ignore_phy,
        ignore_magic,
        crit_odds,
        crit_damage,


        kill_hero_num = 200,//击杀英雄次数
        kill_monster_num,//杀小兵次数
        dead_num,//死亡次数
        assist_num,//助攻次数
        continue_kill_num,//连续击杀次数

        non_control = 300, //不受控
        non_select,//不受选种
        non_move,//不可移动
        non_be_atk,//不可被攻击
        non_visible,//不可见 隐身
        force_visible,//强制显形
        cross_solider,//穿过小兵
        non_obstacle,//无视障碍
        non_skill,//不能放技能

        ///----------------扩展属性 非真实存在---------------
        Extend_Attr = 400,
        hp_percent,
        target_dis,//目标距离
        source_dis,//原点距离
        have_target,
        target_in_rang,
        target_dis_pos,
        fighting,//战斗中
    }
    public class RoleAttrs
    {
        public Role role;
        private Dictionary<int, IAttrValue> dic = new Dictionary<int, IAttrValue>();
        private List<IAttrValue> buffList = new List<IAttrValue>();

        private readonly static ObjectPool<PropAttr> intPool = new ObjectPool<PropAttr>();
        private readonly static ObjectPool<BoolAttr> boolPool = new ObjectPool<BoolAttr>();
        //private readonly static ObjectPool<Observer<RoleAttrs>> attrObsPool = new ObjectPool<Observer<RoleAttrs>>();
        public void UpdateFrame(int delta)
        {
            int count = buffList.Count;
            if (count == 0)
                return;
            for(int i= count-1; i>=0; i--)
            {
                IAttrValue attr = buffList[i];
                if(attr.UpdateValue())
                {
                    this.Change(attr.type);
                    if (attr.Count == 0)
                        buffList.RemoveAt(i);
                }
            }
        }
        public void AddObs(AttrType t,AcHandler<RoleAttrs> ac)
        {
            int k = (int)t;
            if (!dic.ContainsKey(k))
                return;
            IAttrValue attr = dic[k];
            if (attr.obs == null)
                attr.obs = new Observer<RoleAttrs>();
            attr.obs.AddObserver(ac);
        }
        public void RemoveObs(AttrType t, AcHandler<RoleAttrs> ac)
        {
            int k = (int)t;
            if (!dic.ContainsKey(k))
                return;
            IAttrValue attr = dic[k];
            if (attr.obs == null)
                return;
            attr.obs.RemoveObserver(ac);
        }
        protected void Change(AttrType t)
        {
            int k = (int)t;
            if (!dic.ContainsKey(k))
                return;
            IAttrValue attr = dic[k];
            if (attr.obs == null)
                return;
            attr.obs.Change(this);
        }
        public float GetFloat(AttrType t, bool add = false, int min = 0)
        {
            return GetInt(t,add) * PropValue.ratio;
        }
        public int GetInt(AttrType t, bool add = false, int min = 0)
        {
            int v = 0;
            PropAttr p = GetProp(t, add);
            if (p != null)
                v = p.resultValue;
            v = v < min ? min : v;
            return v;
        }
        public PropAttr GetProp(AttrType t, bool add = false)
        {
            int k = (int)t;
            if (dic.ContainsKey(k))
            {
                return (PropAttr)dic[k];
            }
            else if (add)
            {
                PropAttr iv = intPool.Get();
                dic[k] = iv;
                return iv;
            }
            return null;
        }
        public void SetProp(AttrType t, int v, bool isAdd = false)
        {
            SetProp(t,new PropValue(v),isAdd);
        }
        public void SetProp(AttrType t, PropValue v,bool isAdd = false)
        {
            PropAttr list = GetProp(t, true);
            if (isAdd)
                list.value += v;
            else
                list.value = v;
            list.UpdateValue();
            this.Change(t);
        }
        public void AddProp(AttrType t, PropValue v)
        {
            PropAttr list = GetProp(t, true);
            list.AddValue(v);
            buffList.Add(list);
            this.Change(t);
        }
        public bool RemoveProp(AttrType t, int id)
        {
            PropAttr list = GetProp(t, true);
            bool b = list.RemoveValue(id);
            if(b)
                this.Change(t);
            return b;
        }
        public bool GetBoolV(AttrType t, bool add = false)
        {
            BoolAttr att = GetBool(t, add);
            return att == null ? false : att.resultValue;
        }
        public BoolAttr GetBool(AttrType t, bool add = false)
        {
            int k = (int)t;
            if (dic.ContainsKey(k))
            {
                return (BoolAttr)dic[k];
            }
            else if(add)
            {
                BoolAttr iv = boolPool.Get();
                dic[k] = iv;
                return iv;
            }
            return null;
        }
        public bool SetBool(AttrType t, bool b,int lv = 0)
        {
            return SetBool(t, new BoolValue(b, lv));
        }
        public bool SetBool(AttrType t, BoolValue v)
        {
            BoolAttr list = GetBool(t, true);
            if (v.level < list.value.level)
                return false;
            list.value = v;
            list.UpdateValue();
            this.Change(t);
            return true;
        }
        public void AddBool(AttrType t, BoolValue v)
        {
            BoolAttr list = GetBool(t, true);
            list.AddValue(v);
            buffList.Add(list);
            this.Change(t);
        }
        public bool RemoveBool(AttrType t, int id)
        {
            BoolAttr list = GetBool(t, true);
            bool b = list.RemoveValue(id);
            if(b)
                this.Change(t);
            return b;
        }
        private readonly static ObjectPool<RoleAttrs> pool = new ObjectPool<RoleAttrs>();
        public static RoleAttrs Get(Role _obj)
        {
            RoleAttrs attrs = pool.Get();
            attrs.role = _obj;
            return attrs;
        }
        public void Release()
        {
            foreach (var v in dic.Values)
            {
                v.ClearValue();
                if (v is PropAttr)
                    intPool.Release((PropAttr)v);
                else if (v is BoolAttr)
                    boolPool.Release((BoolAttr)v);
            }
            dic.Clear();
            buffList.Clear();
            role = null;
            pool.Release(this);
        }

        public int hp_percent
        {
            get
            {
                int hp = GetProp(AttrType.hp,true).resultValue;
                int max_hp = GetProp(AttrType.max_hp, true).resultValue;
                return hp * 1000/ max_hp;
            }
        }

    }
}