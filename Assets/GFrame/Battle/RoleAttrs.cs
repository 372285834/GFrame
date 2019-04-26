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
        void ClearValue();
    }
    public interface IPropAttrValue
    {
        PropValue GetValue();
    }
    public interface IBoolAttrValue
    {
        BoolValue GetValue();
    }
    public class PropAttr : AttrValue<PropValue, IPropAttrValue> {
        public override PropValue GetValue()
        {
            tempValue = this.value;
            for (int i = 0; i < this.Count; i++)
            {
                tempValue += this[i].GetValue();
            }
            return tempValue;
        }
    }
    public class BoolAttr : AttrValue<BoolValue, IBoolAttrValue>
    {
        public override BoolValue GetValue()
        {
            tempValue = this.value;
            for (int i = 0; i < this.Count; i++)
            {
                BoolValue cur = this[i].GetValue();
                if (cur.level > tempValue.level)
                    tempValue = cur;
            }
            return tempValue;
        }
    }
    public class AttrValue<T,V> : List<V>, IAttrValue
    {
        public T value;
        public T tempValue;
        public virtual T GetValue()
        {
            tempValue = value;
            return value;
        }
        public virtual void AddValue(V v)
        {
            this.Add(v);
            this.GetValue();
        }
        public virtual bool RemoveValue(V v)
        {
            bool b = this.Remove(v);
            this.GetValue();
            return b;
        }
        public virtual void ClearValue()
        {
            this.Clear();
        }
    }
    public struct BoolValue
    {
        public int level;
        public bool value;
        public BoolValue(bool v,int lv=0)
        {
            value = v;
            level = lv;
        }
    }
    public struct PropValue
    {
        static float ratio = 0.001f;
        public readonly static PropValue zero = new PropValue(0);
        public int baseValue;
        public int extraValue;
        public int basePer;
        public int totalPer;
        public PropValue(int bv)
        {
            baseValue = bv;
            extraValue = 0;
            basePer = 0;
            totalPer = 0;
        }
        public PropValue(int bv, int ev, int br, int er)
        {
            baseValue = bv;
            extraValue = ev;
            basePer = br;
            totalPer = er;
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
                int baseR = (int)(ratio * this.baseValue * this.basePer);
                int totalR = (int)(ratio * aValue * this.totalPer);
                return aValue + baseR + totalR;
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
        move_speed = 1,
        max_hp,
        max_mp,
        hp,
        mp,
        recovery_hp,
        recovery_mp,
        def_phy,
        def_magic,
        toughness,//韧性 减少控制时间
        def_reduce_speed,//减少减速时间
        dodge,//闪避

        hit_rate,//命中率
        atk_speed,
        atk_phy,
        atk_magic,
        ignore_phy,
        ignore_magic,
        crit,
        crit_odds,

        reduce_cd,

        kill_hero_num = 100,//击杀英雄次数
        kill_monster_num,//杀小兵次数
        dead_num,//死亡次数
        assist_num,//助攻次数
        continue_kill_num,//连续击杀次数

        non_control = 200, //不受控
        non_select,//不受选种
        non_move,//不可移动
        non_atk,//不可攻击
        non_visible,//不可见 隐身
        force_visible,//强制显形
        cross_solider,//穿过小兵
        non_obstacle,//无视障碍
    }
    public enum RoleObsType
    {
        Prop,
        Bool,
        Num,
        Hit,//受到攻击
        Attack,//攻击别人
    }
    public  class RoleAttrs
    {
        public Role role;
        private Dictionary<int, object> dic = new Dictionary<int, object>();
        private Observer<RoleObsType, object> obs = new Observer<RoleObsType, object>();

        private readonly static ObjectPool<PropAttr> intPool = new ObjectPool<PropAttr>();
        private readonly static ObjectPool<BoolAttr> boolPool = new ObjectPool<BoolAttr>();

        public void AddObs(AcHandler<RoleObsType, object> ac)
        {
            obs.AddObserver(ac);
        }
        public void RemoveObs(AcHandler<RoleObsType, object> ac)
        {
            obs.RemoveObserver(ac);
        }
        protected void Change(RoleObsType t, object o)
        {
            obs.Change(t, o);
        }
        public PropAttr GetProp(AttrType t, bool add = false)
        {
            object v = null;
            dic.TryGetValue((int)t, out v);
            if (v == null && add)
            {
                PropAttr iv = intPool.Get();
                v = iv;
                dic[(int)t] = v;
            }
            return (PropAttr)v;
        }
        public void CalcProp(AttrType t, PropValue v)
        {
            PropAttr list = GetProp(t, true);
            list.value += v;
            list.GetValue();
            this.Change(RoleObsType.Prop, list);
        }
        public void AddProp(AttrType t, IPropAttrValue v)
        {
            PropAttr list = GetProp(t, true);
            list.AddValue(v);
            this.Change(RoleObsType.Prop, list);
        }
        public bool RemoveProp(AttrType t, IPropAttrValue v)
        {
            PropAttr list = GetProp(t, true);
            bool b = list.RemoveValue(v);
            this.Change(RoleObsType.Prop, list);
            return b;
        }
        public BoolAttr GetBool(AttrType t, bool add = false)
        {
            object v = null;
            dic.TryGetValue((int)t, out v);
            if (v == null && add)
            {
                BoolAttr iv = boolPool.Get();
                v = iv;
                dic[(int)t] = v;
            }
            return (BoolAttr)v;
        }
        public void AddBool(AttrType t, IBoolAttrValue v)
        {
            BoolAttr list = GetBool(t, true);
            list.AddValue(v);
            this.Change(RoleObsType.Bool, list);
        }
        public bool RemoveBool(AttrType t, IBoolAttrValue v)
        {
            BoolAttr list = GetBool(t, true);
            bool b = list.RemoveValue(v);
            this.Change(RoleObsType.Bool, list);
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
                if(v is IAttrValue)
                    (v as IAttrValue).ClearValue();
                if (v is PropAttr)
                    intPool.Release((PropAttr)v);
                else if (v is BoolAttr)
                    boolPool.Release((BoolAttr)v);
            }
            dic.Clear();
            obs.Clear();
            role = null;
            pool.Release(this);
        }
    }
}