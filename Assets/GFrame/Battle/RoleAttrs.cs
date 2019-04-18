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
    public class IntAttr : AttrValue<IntValue> { }
    public class BoolAttr : AttrValue<BoolValue>
    {
        public override BoolValue OnChange(BoolValue t)
        {
            for (int i = mList.Count - 1; i >= 0; i--)
            {
                t = mList[i](t);
                if (t.value)
                    return t;
            }
            return t;
        }
    }
    public class AttrValue<T> : ObserverV<T>
    {
        public T value;
        public T GetValue()
        {
            T v = this.Change(this.value);
            return v;
        }
    }
    public struct BoolValue
    {
        public bool value;
    }
    public struct IntValue
    {
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
                int baseR = this.baseValue * this.basePer;
                int totalR = aValue * this.totalPer;
                return aValue + baseR + totalR;
            }
        }

        public float showValue
        {
            get { return value * 0.001f; }
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
    public enum IntAttrType
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

    }
    public enum BoolAttrType
    {
        non_control = 1,
        non_select,
        non_move,
        non_atk,
        non_visible,
    }
    public class RoleAttrs
    {
        private readonly static ObjectPool<RoleAttrs> pool = new ObjectPool<RoleAttrs>();
        public static RoleAttrs Get(SceneObject _obj)
        {
            RoleAttrs skills = pool.Get();
            skills.obj = _obj;
            return skills;
        }
        public void Release()
        {
            foreach (var v in dic.Values)
            {
                v.Clear();
            }
            obj = null;
            pool.Release(this);
        }
        public SceneObject obj;
        public Dictionary<IntAttrType, IntAttr> dic;
        public IntAttr max_hp;
        public IntAttr max_mp;
        public IntAttr hp;
        public IntAttr mp;
        public IntAttr recovery_hp;
        public IntAttr recovery_mp;
        public IntAttr def_phy;
        public IntAttr def_magic;
        public IntAttr toughness;//韧性 减少控制时间
        public IntAttr def_reduce_speed;//减少减速时间
        public IntAttr dodge;//闪避

        public IntAttr hit;//命中
        public IntAttr atk_speed;
        public IntAttr atk_phy;
        public IntAttr atk_magic;
        public IntAttr ignore_phy;
        public IntAttr ignore_magic;
        public IntAttr crit;
        public IntAttr crit_odds;

        public IntAttr move_speed;
        public IntAttr reduce_cd;

        public BoolAttr non_control; //不受控
        public BoolAttr non_select;//不受选种
        public BoolAttr non_move;//不可移动
        public BoolAttr non_atk;//不可攻击
        public BoolAttr non_visible;//不可见
        public void UpdateFrame(int frame)
        {

        }
        public IntAttr GetIntAttr(IntAttrType t)
        {
            IntAttr v = null;
            dic.TryGetValue(t, out v);
            return v;
        }
        public BoolAttr GetBoolAttr(BoolAttrType t)
        {
            if (t == BoolAttrType.non_move)
                return non_move;
            if (t == BoolAttrType.non_atk)
                return non_atk;
            if (t == BoolAttrType.non_visible)
                return non_visible;
            if (t == BoolAttrType.non_control)
                return this.non_control;
            if (t == BoolAttrType.non_select)
                return non_select;
            return null;
        }
        public void Init()
        {

        }
    }
}