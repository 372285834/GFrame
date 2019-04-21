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
        AttrType type { get; set; }
        void ClearValue();
    }
    public interface IIntAttrValue
    {
        IntValue GetValue();
    }
    public interface IBoolAttrValue
    {
        BoolValue GetValue();
    }
    public class IntAttr : AttrValue<IntValue, IIntAttrValue> {
        public override IntValue GetValue()
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
        public AttrType type { get; set; }
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
    public struct IntValue
    {
        static float ratio = 0.001f;
        public readonly static IntValue zero = new IntValue(0);
        public int baseValue;
        public int extraValue;
        public int basePer;
        public int totalPer;
        public IntValue(int bv)
        {
            baseValue = bv;
            extraValue = 0;
            basePer = 0;
            totalPer = 0;
        }
        public IntValue(int bv, int ev, int br, int er)
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
        public static IntValue operator +(IntValue a, IntValue b)
        {
            IntValue c = a;
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

        hit,//命中
        atk_speed,
        atk_phy,
        atk_magic,
        ignore_phy,
        ignore_magic,
        crit,
        crit_odds,

        reduce_cd,


        non_control = 100, //不受控
        non_select,//不受选种
        non_move,//不可移动
        non_atk,//不可攻击
        non_visible,//不可见 隐身
        force_visible,//强制显形
    }
    public  class RoleAttrs
    {
        public Role role;
        private Dictionary<int, IAttrValue> dic = new Dictionary<int, IAttrValue>();
        private ObserverV<IAttrValue> obs_value = new ObserverV<IAttrValue>();

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
        public IntAttr GetIntAttr(AttrType t, bool add = false)
        {
            IAttrValue v = null;
            dic.TryGetValue((int)t, out v);
            if (v == null && add)
            {
                IntAttr iv = intPool.Get();
                iv.type = t;
                v = iv;
                dic[(int)t] = v;
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
        public void AddIntAttr(AttrType t, IIntAttrValue v)
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
            dic.TryGetValue((int)t, out v);
            if (v == null && add)
            {
                BoolAttr iv = boolPool.Get();
                iv.type = t;
                v = iv;
                dic[(int)t] = v;
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
                if (v is IntAttr)
                    intPool.Release((IntAttr)v);
                else if (v is IntAttr)
                    boolPool.Release((BoolAttr)v);
            }
            dic.Clear();
            obs_value.Clear();
            role = null;
            pool.Release(this);
        }
    }
}